namespace BasketSurvay.Contracts.Roles;

public class RoleRequestValidator : AbstractValidator<RoleRequest>
{
    public RoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("role name is required")
            .Length(3, 256).WithMessage("length must be between 3 & 256 characters");

        RuleFor(x => x.Permissions)
            .NotNull().NotEmpty().WithMessage("at least one permission is required");

        RuleFor(x => x.Permissions)
            .Must(x => x.Distinct().Count() == x.Count()).WithMessage("douplicated permissions had been found")
            .When(x => x.Permissions is not null); //to force it stop when the list is finished 

    }
}
