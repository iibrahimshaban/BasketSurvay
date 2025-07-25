using BasketSurvay.Authentication;
using BasketSurvay.Helpers;
using Hangfire;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;
using System.Text;

namespace BasketSurvay.Services
{
    public class AuthService(UserManager<ApplicationUser> userManager
        , SignInManager<ApplicationUser> signInManager
        , IJwtProvider jwt
        , ILogger<AuthService> logger
        , IEmailSender emailSender,
        IHttpContextAccessor httpContextAccessor
        , ApplicationDbContext context) : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IJwtProvider _jwtProvider = jwt;
        private readonly ILogger<AuthService> _logger = logger;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ApplicationDbContext _context = context;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;

        private readonly int _RefreshTokenExpiryDays = 14;
        public async Task<Result<AuthenResponse>> GetTokenAsync(string Email, string Password, CancellationToken cancellationToken = default)
        {
            if (await _userManager.FindByEmailAsync(Email) is not { } User)
                return Result.Failure<AuthenResponse>(UserErrors.InvalidCredintials);

            if (User.IsDisabled)
                return Result.Failure<AuthenResponse>(UserErrors.DisabledUser);

            var result = await _signInManager.PasswordSignInAsync(User, Password, false, true);

            if (result.Succeeded)
            {
                (var userRoles, var userPermissions) = await GetUserRolesAndPermissions(User, cancellationToken);

                var (Token, ExpiresIn) = _jwtProvider.GenerateToken(User, userRoles, userPermissions!);

                var RefreshToken = GenerateRefreshToken();
                var RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(_RefreshTokenExpiryDays);

                User.RefreshTokens.Add(new RefreshToken
                {
                    Token = RefreshToken,
                    ExpiresOn = RefreshTokenExpiryDate,
                });

                await _userManager.UpdateAsync(User);

                var response = new AuthenResponse(User.Id, User.Email, User.FirstName, User.LastName, Token, ExpiresIn,
                    RefreshToken, RefreshTokenExpiryDate);

                return Result.Success(response);
            }

            if (result.IsLockedOut)
                return Result.Failure<AuthenResponse>(UserErrors.LockedOutUser);

            return Result.Failure<AuthenResponse>(result.IsNotAllowed
                        ? UserErrors.EmailNotConfirmed
                        : UserErrors.InvalidCredintials);
        }
        public async Task<OneOf<AuthenResponse, Error>> GetRefreshTokenAsync(string Token, string Refreshtoken,
            CancellationToken cancellationToken = default)
        {
            var UserId = _jwtProvider.ValidateToken(Token);

            if (UserId is null)
                return UserErrors.InvalidRefreshCredintials;

            var User = await _userManager.FindByIdAsync(UserId);

            if (User is null)
                return UserErrors.InvalidRefreshCredintials;

            if (User.IsDisabled)
                return UserErrors.DisabledUser;

            if (User.LockoutEnd > DateTime.UtcNow)
                return UserErrors.LockedOutUser;

            var UserRefreshToken = User.RefreshTokens
                .SingleOrDefault(x => x.Token == Refreshtoken && x.IsActivated);

            if (UserRefreshToken is null)
                return UserErrors.InvalidRefreshCredintials;
            //give a revoke date 
            UserRefreshToken.RevokedOn = DateTime.UtcNow;

            (var userRoles, var userPermissions) = await GetUserRolesAndPermissions(User, cancellationToken);

            var (NewToken, ExpiresIn) = _jwtProvider.GenerateToken(User, userRoles, userPermissions);

            var NewRefreshToken = GenerateRefreshToken();
            var RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(_RefreshTokenExpiryDays);

            User.RefreshTokens.Add(new RefreshToken
            {
                Token = NewRefreshToken,
                ExpiresOn = RefreshTokenExpiryDate,
            });

            await _userManager.UpdateAsync(User);

            return new AuthenResponse(User.Id, User.Email, User.FirstName, User.LastName, NewToken, ExpiresIn,
                NewRefreshToken, RefreshTokenExpiryDate);

        }
        public async Task<Result> RevokeRefreshTokenAsync(string Token, string Refreshtoken,
            CancellationToken cancellationToken = default)
        {
            var UserId = _jwtProvider.ValidateToken(Token);

            if (UserId is null)
                return Result.Failure(UserErrors.InvalidRefreshCredintials);

            var User = await _userManager.FindByIdAsync(UserId);

            if (User is null)
                return Result.Failure(UserErrors.InvalidRefreshCredintials);

            var UserRefreshToken = User.RefreshTokens
                .SingleOrDefault(x => x.Token == Refreshtoken && x.IsActivated);

            if (UserRefreshToken is null)
                return Result.Failure(UserErrors.InvalidRefreshCredintials);
            //revoke refresh token 
            UserRefreshToken.RevokedOn = DateTime.UtcNow;

            await _userManager.UpdateAsync(User);

            return Result.Success();
        }

        public async Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
        {
            var UserIsExists = await _userManager.Users.AnyAsync(x => x.Email == request.Email, cancellationToken);

            if (UserIsExists)
                return Result.Failure(UserErrors.DuplicatedEmail);

            var UserNameIsExists = await _userManager.Users.AnyAsync(x => x.UserName == request.UserName, cancellationToken);

            if (UserIsExists)
                return Result.Failure(UserErrors.DuplicatedUsername);


            var User = new ApplicationUser
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.UserName,
            };

            var result = await _userManager.CreateAsync(User, request.Password);

            if (result.Succeeded)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(User);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                _logger.LogInformation("Verification code : {code}", code);

                await SendConfirmationEmailAsync(User, code);

                return Result.Success();
            }

            var error = result.Errors.FirstOrDefault();

            return Result.Failure(new Error(error!.Code, error.Description, StatusCodes.Status400BadRequest));

        }

        public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken)
        {
            var User = await _userManager.FindByIdAsync(request.UserId);

            if (User == null)
                return Result.Failure(UserErrors.InvalidCode);

            if (User.EmailConfirmed)
                return Result.Failure(UserErrors.DuplicatedConfirmation);

            var code = request.Code;
            try
            {
                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            }
            catch (FormatException)
            {
                return Result.Failure(UserErrors.InvalidCode);
            }

            var result = await _userManager.ConfirmEmailAsync(User, code);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(User, DefaultRoles.Member.Name);
                return Result.Success();
            }


            var error = result.Errors.FirstOrDefault();

            return Result.Failure(new Error(error!.Code, error.Description, StatusCodes.Status400BadRequest));

        }

        public async Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request, CancellationToken cancellationToken)
        {

            if (await _userManager.FindByEmailAsync(request.Email) is not { } user)
                return Result.Success();

            if (user.EmailConfirmed)
                return Result.Failure(UserErrors.DuplicatedConfirmation);

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            _logger.LogInformation("verification code : {code}", code);

            await SendConfirmationEmailAsync(user, code);

            return Result.Success();
        }
        public async Task<Result> SendResetPasswordCodeAsync(ForgetPasswordRequest request, CancellationToken cancellationToken)
        {
            if (await _userManager.FindByEmailAsync(request.Email) is not { } user)
                return Result.Success();

            if (!user.EmailConfirmed)
                return Result.Failure(UserErrors.EmailNotConfirmed);

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            _logger.LogInformation("reset code : {code}", code);

            await SendResetPasswordEmailAsync(user, code);

            return Result.Success();
        }

        public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            if (await _userManager.FindByEmailAsync(request.Email) is not { } user)
                return Result.Success();

            if (!user.EmailConfirmed)
                return Result.Failure(UserErrors.EmailNotConfirmed with { StatusCode = StatusCodes.Status400BadRequest });

            IdentityResult result;

            try
            {
                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
                result = await _userManager.ResetPasswordAsync(user, code, request.NewPassword);
            }
            catch
            {
                result = IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
            }

            if (result.Succeeded)
                return Result.Success();

            var error = result.Errors.First();

            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status401Unauthorized));
        }
        private static string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
        private async Task SendConfirmationEmailAsync(ApplicationUser User, string code)
        {
            //we won't use origin as i can't send url with the headers 
            var origin = _httpContextAccessor.HttpContext?.Request.Headers.Host;

            _logger.LogInformation("url is {origin}", origin);
            var EmailBody = EmailBodyBuilder.GenerateEmailBody("EmailConfirmation",
                new Dictionary<string, string>
                {
                        { "{{name}}",User.FirstName+" "+User.LastName},
                        { "{{action_url}}",$"https://{origin}/Auth/Confirm-Email?userId={User.Id}&code={code}" }
                });

            BackgroundJob.Enqueue(() =>
            _emailSender.SendEmailAsync(User.Email!, "✅ Survay basket : email verification ", EmailBody));

            await Task.CompletedTask;

        }
        private async Task SendResetPasswordEmailAsync(ApplicationUser User, string code)
        {
            var origin = _httpContextAccessor.HttpContext?.Request.Headers.Host;

            _logger.LogInformation("url is {origin}", origin);
            var EmailBody = EmailBodyBuilder.GenerateEmailBody("ForgetPassword",
                new Dictionary<string, string>
                {
                        { "{{name}}",User.FirstName+" "+User.LastName},
                        { "{{action_url}}",$"https://{origin}/Auth/forget-password?email={User.Email}&code={code}" }
                });

            BackgroundJob.Enqueue(() =>
            _emailSender.SendEmailAsync(User.Email!, "✅ Survay basket : reset password ", EmailBody));

            await Task.CompletedTask;
        }

        private async Task<(IEnumerable<string> Roles, IEnumerable<string> Permissions)> GetUserRolesAndPermissions(ApplicationUser User, CancellationToken cancellationToken)
        {
            var userRoles = await _userManager.GetRolesAsync(User);



            var userPermissions = await (from role in _context.Roles
                                         join claim in _context.RoleClaims
                                         on role.Id equals claim.RoleId
                                         where userRoles.Contains(role.Name!)
                                         select claim.ClaimValue)
                                         .Distinct()
                                         .ToListAsync(cancellationToken);

            return (userRoles, userPermissions!);
        }

    }
}
