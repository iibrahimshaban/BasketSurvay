namespace BasketSurvay.Contracts.Users;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.Currentpassword)
               .NotEmpty().WithMessage("Password is required.")
               .Matches(RegexPattern.Password).WithMessage("password at least must be 8-digits and a complex password");

        RuleFor(x => x.Newpassword)
               .NotEmpty().WithMessage("Password is required.")
               .Matches(RegexPattern.Password).WithMessage("password at least must be 8-digits and a complex password")
               .NotEqual(x => x.Currentpassword).WithMessage("New password can't be same as the current password");
    }
}
