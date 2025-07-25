namespace BasketSurvay.Contracts.Users;

public record UserResponse(
    string Id,
    string UserName,
    string FirstName,
    string LastName,
    string Email,
    bool IsDisabled,
    IEnumerable<string> Roles
    );

