namespace BasketSurvay.Authentication
{
    public interface IJwtProvider
    {
        (string Token, int ExpiresIn) GenerateToken(ApplicationUser user, IEnumerable<string> Roles, IEnumerable<string> Permissions);
        string? ValidateToken(string token);
    }
}
