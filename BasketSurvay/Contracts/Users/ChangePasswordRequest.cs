namespace BasketSurvay.Contracts.Users;

public record ChangePasswordRequest(
    string Currentpassword,
    string Newpassword
    );
