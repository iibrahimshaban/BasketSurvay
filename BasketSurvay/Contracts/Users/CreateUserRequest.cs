namespace BasketSurvay.Contracts.Users;

public record CreateUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string UserName,
    IList<string> Roles
    );

