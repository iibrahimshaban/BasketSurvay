namespace BasketSurvay.Contracts.Users;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {

        RuleFor(x => x.FirstName)
               .NotEmpty().WithMessage("first name  is required.")
               .Length(3, 100).WithMessage("First name must be greatter then 3 chars and less thean 100 char");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("last name is required.")
            .Length(3, 100).WithMessage("last name must be greatter then 3 chars and less thean 100 char");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Password is required.")
            .Matches(RegexPattern.UserName).WithMessage("username must start with a letter and can contain only letters, numbers, or underscores.");

    }
}
