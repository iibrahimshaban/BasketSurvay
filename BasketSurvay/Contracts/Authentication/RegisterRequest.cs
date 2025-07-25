namespace BasketSurvay.Contracts.Authentication
{
    public record RegisterRequest(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string UserName
        );

}
