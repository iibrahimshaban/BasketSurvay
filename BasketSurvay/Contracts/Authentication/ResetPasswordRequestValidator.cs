namespace BasketSurvay.Contracts.Authentication;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
               .NotEmpty().WithMessage("Email is required.")
               .EmailAddress().WithMessage("Email must be in a valid format.");

        RuleFor(x => x.Code)
              .NotEmpty().WithMessage("code is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Password is required.")
            .Matches(RegexPattern.Password).WithMessage("password at least must be 8-digits and a complex password");

    }
}
