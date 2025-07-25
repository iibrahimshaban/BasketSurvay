namespace BasketSurvay.Contracts.Users;

public record UpdateUserRequest(
    string Email,
    string UserName,
    string FirstName,
    string LastName,
    IList<string> Roles
    );

