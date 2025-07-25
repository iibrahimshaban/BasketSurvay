namespace BasketSurvay.Errors;

public static class RoleErrors
{

    public static readonly Error RoleNotFound =
        new("Role.NotFound", "can't find Role with given Details", StatusCodes.Status404NotFound);

    public static readonly Error InvalidPermissions =
        new("Role.InvalidPermissions", "these permissions is out of the boundary", StatusCodes.Status400BadRequest);

    public static readonly Error DuplicatedRole =
           new("Role.Duplicated", "there is already a Role with the same Name", StatusCodes.Status409Conflict);

}
