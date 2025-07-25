namespace BasketSurvay.Contracts.Users;

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string UserName
    );
