

namespace BasketSurvay.Services
{
    public interface IAuthService
    {
        Task<Result<AuthenResponse>> GetTokenAsync(string Email, string Password,
            CancellationToken cancellationToken = default);
        Task<OneOf<AuthenResponse, Error>> GetRefreshTokenAsync(string Token, string Refreshtoken,
            CancellationToken cancellationToken = default);
        Task<Result> RevokeRefreshTokenAsync(string Token, string Refreshtoken,
            CancellationToken cancellationToken = default);
        Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
        Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken);
        Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request, CancellationToken cancellationToken);
        Task<Result> SendResetPasswordCodeAsync(ForgetPasswordRequest request, CancellationToken cancellationToken);
        Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken);

    }

}
