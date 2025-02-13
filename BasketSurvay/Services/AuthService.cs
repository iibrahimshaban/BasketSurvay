
using BasketSurvay.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace BasketSurvay.Services
{
    public class AuthService(UserManager<ApplicationUser> userManager,IJwtProvider jwt) : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IJwtProvider _jwtProvider = jwt;

        private readonly int _RefreshTokenExpiryDays = 14;
        public async Task<AuthenResponse?> GetTokenAsync(string Email, string Password, CancellationToken cancellationToken = default)
        {
            var User = await _userManager.FindByEmailAsync(Email);

            if (User is null)
                return null;

            var PasswordChecked = await _userManager.CheckPasswordAsync(User, Password);

            if (!PasswordChecked)
                return null;

            var (Token, ExpiresIn) = _jwtProvider.GenerateToken(User);

            var RefreshToken = GenerateRefreshToken();
            var RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(_RefreshTokenExpiryDays);

            User.RefreshTokens.Add(new RefreshToken
            {
                Token = RefreshToken,
                ExpiresOn = RefreshTokenExpiryDate,
            });

            await _userManager.UpdateAsync(User);

            return new AuthenResponse(User.Id,User.Email,User.FirstName,User.LastName,Token,ExpiresIn,
                RefreshToken,RefreshTokenExpiryDate);
        }
        public async Task<AuthenResponse?> GetRefreshTokenAsync(string Token, string Refreshtoken,
            CancellationToken cancellationToken = default)
        {
            var UserId = _jwtProvider.ValidateToken(Token);

            if (UserId is null)
                return null;

            var User = await _userManager.FindByIdAsync(UserId);

            if (User is null)
                return null;

            var UserRefreshToken = User.RefreshTokens
                .SingleOrDefault(x => x.Token == Refreshtoken && x.IsActivated);

            if (UserRefreshToken is null) 
                return null;
            //give a revoke date 
            UserRefreshToken.RevokedOn = DateTime.UtcNow;

            var (NewToken, ExpiresIn) = _jwtProvider.GenerateToken(User);

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
        public async Task<bool> RevokeRefreshTokenAsync(string Token, string Refreshtoken,
            CancellationToken cancellationToken = default)
        {
            var UserId = _jwtProvider.ValidateToken(Token);

            if (UserId is null)
                return false;

            var User = await _userManager.FindByIdAsync(UserId);

            if (User is null)
                return false;

            var UserRefreshToken = User.RefreshTokens
                .SingleOrDefault(x => x.Token == Refreshtoken && x.IsActivated);

            if (UserRefreshToken is null)
                return false;
            //revoke refresh token 
            UserRefreshToken.RevokedOn = DateTime.UtcNow;

            await _userManager.UpdateAsync(User);
            return true;
        }
        private static string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        
    }
}
