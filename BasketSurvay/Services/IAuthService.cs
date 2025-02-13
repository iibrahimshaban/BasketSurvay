

namespace BasketSurvay.Services
{
    public interface IAuthService
    {
        Task<AuthenResponse?> GetTokenAsync(string Email ,string Password ,
            CancellationToken cancellationToken= default);
        Task<AuthenResponse?> GetRefreshTokenAsync(string Token,string Refreshtoken ,
            CancellationToken cancellationToken= default);

        Task<bool> RevokeRefreshTokenAsync(string Token, string Refreshtoken,
            CancellationToken cancellationToken = default);
    }

}
