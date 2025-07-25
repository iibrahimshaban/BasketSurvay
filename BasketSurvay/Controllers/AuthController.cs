namespace BasketSurvay.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        private readonly ILogger<AuthController> _logger = logger;

        [HttpPost("")]
        public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken = default)
        {
            _logger
                .LogInformation("Logging with [Email: {email} ,Password: {password}]", request.Email, request.Password);

            var LoginResult = await _authService.GetTokenAsync(request.Email, request.Password, cancellationToken);

            return LoginResult.IsSuccess
                ? Ok(LoginResult.Value)
                : LoginResult.Error.ToProblem();
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest refresh,
            CancellationToken cancellationToken)
        {
            var AuthResult = await _authService.GetRefreshTokenAsync(refresh.Token
                , refresh.RefreshToken, cancellationToken);

            return AuthResult.Match(
                AuthResponse => Ok(AuthResponse),
                error => error.ToProblem()
                );
        }
        [HttpPost("revoke-refresh-token")]
        public async Task<IActionResult> RevokeRefreshToken([FromBody] RefreshTokenRequest refresh,
            CancellationToken cancellationToken)
        {
            var result = await _authService.RevokeRefreshTokenAsync(refresh.Token,
                refresh.RefreshToken, cancellationToken);

            return result.IsSuccess
                ? Ok(result.IsSuccess) :
                result.Error.ToProblem();
        }
        [HttpPost("registration")]
        public async Task<IActionResult> SignUp([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(request, cancellationToken);

            return result.IsSuccess
                ? Ok()
                : result.Error.ToProblem();
        }
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmEmailRequest request
            , CancellationToken cancellationToken)
        {
            var result = await _authService.ConfirmEmailAsync(request, cancellationToken);

            return result.IsSuccess
                ? Ok()
                : result.Error.ToProblem();
        }
        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailRequest request
            , CancellationToken cancellationToken)
        {
            var result = await _authService.ResendConfirmationEmailAsync(request, cancellationToken);

            return result.IsSuccess
                ? Ok()
                : result.Error.ToProblem();
        }
        [HttpPost("forget-password")]
        public async Task<IActionResult> Forgetpassword([FromBody] ForgetPasswordRequest request
            , CancellationToken cancellationToken)
        {
            var result = await _authService.SendResetPasswordCodeAsync(request, cancellationToken);

            return result.IsSuccess
                ? Ok()
                : result.Error.ToProblem();
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request
            , CancellationToken cancellationToken)
        {
            var result = await _authService.ResetPasswordAsync(request, cancellationToken);

            return result.IsSuccess
                ? Ok()
                : result.Error.ToProblem();
        }

    }
}
