using BasketSurvay.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BasketSurvay.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("")]
        public async Task<IActionResult> LoginAsync(LoginRequest request,CancellationToken cancellationToken = default)  
        {
            var LoginResult = await _authService.GetTokenAsync(request.Email, request.Password,cancellationToken);

            return LoginResult is null ? BadRequest("invalid user/password") : Ok(LoginResult);
        }
        [HttpPut("Refresh")]
        public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequest refresh,
            CancellationToken cancellationToken)
        {
            var AuthResult = await _authService.GetRefreshTokenAsync(refresh.Token
                , refresh.RefreshToken, cancellationToken);

            return AuthResult is null ? BadRequest("invalid token"):Ok(AuthResult);
        }
        [HttpPost("Revoke-Refresh-Token")]
        public async Task<IActionResult> RevokeRefreshTokenAsync([FromBody] RefreshTokenRequest refresh,
            CancellationToken cancellationToken)
        {
            var IsRevoked = await _authService.RevokeRefreshTokenAsync(refresh.Token,
                refresh.RefreshToken, cancellationToken);

            return IsRevoked ?Ok(IsRevoked): BadRequest("invalid refresh token ");
        }
        
    }
}
