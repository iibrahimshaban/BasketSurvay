namespace BasketSurvay.Contracts.Authentication
{
    public record AuthenResponse(
        string Id,
        string? Email,
        string FName,
        string LName,
        string Token,
        int ExpiresIn,
        string RefreshToken,
        DateTime RefreshTokenExpirationdate
     );

}
