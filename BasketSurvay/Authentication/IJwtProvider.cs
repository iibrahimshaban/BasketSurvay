namespace BasketSurvay.Authentication
{
    public interface IJwtProvider
    {
        (string Token,int ExpiresIn) GenerateToken(ApplicationUser user);
        string? ValidateToken(string token);
    }
}
