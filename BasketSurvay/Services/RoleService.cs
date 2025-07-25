
namespace BasketSurvay.Services;

public class RoleService(RoleManager<ApplicationRole> roleManager, ApplicationDbContext context) : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<RoleResponse>> GetAllAsync(bool? includeDisable = false)
    {
        return await _roleManager.Roles
            .Where(x => !x.IsDefault && (!x.IsDeleted || (includeDisable.HasValue && includeDisable.Value)))
            .AsNoTracking()
            .ProjectToType<RoleResponse>()
            .ToListAsync();
    }

    public async Task<Result<RoleDetailResponse>> GetDetailsAsync(string roleId)
    {
        if (await _roleManager.FindByIdAsync(roleId) is not { } role)
            return Result.Failure<RoleDetailResponse>(RoleErrors.RoleNotFound);

        var permissions = await _roleManager.GetClaimsAsync(role);

        var response = new RoleDetailResponse(role.Id, role.Name!, role.IsDeleted, permissions.Select(x => x.Value));

        return Result.Success(response);
    }

    public async Task<Result<RoleDetailResponse>> CreateAsync(RoleRequest request)
    {
        var RoleExists = await _roleManager.RoleExistsAsync(request.Name);

        if (RoleExists)
            return Result.Failure<RoleDetailResponse>(RoleErrors.DuplicatedRole);

        var allowedPermissions = Permissions.GetAllPermissions();

        if (request.Permissions.Except(allowedPermissions).Any())
            return Result.Failure<RoleDetailResponse>(RoleErrors.InvalidPermissions);

        var role = new ApplicationRole
        {
            Name = request.Name,
            ConcurrencyStamp = Guid.CreateVersion7().ToString()
        };

        var result = await _roleManager.CreateAsync(role);

        if (result.Succeeded)
        {
            var permissions = request.Permissions
                .Select(x => new IdentityRoleClaim<string>
                {
                    ClaimType = Permissions.Type,
                    ClaimValue = x,
                    RoleId = role.Id
                });
            await _context.RoleClaims.AddRangeAsync(permissions);
            await _context.SaveChangesAsync();

            var response = new RoleDetailResponse(role.Id, role.Name, role.IsDeleted, request.Permissions);

            return Result.Success(response);
        }

        var error = result.Errors.First();

        return Result.Failure<RoleDetailResponse>(new
            Error(error.Code, error.Description, StatusCodes.Status400BadRequest));

    }

    public async Task<Result> updateAsync(string id, RoleRequest request)
    {
        if (await _roleManager.FindByIdAsync(id) is not { } role)
            return Result.Failure(RoleErrors.RoleNotFound);

        var NameExisits = await _roleManager.Roles.AnyAsync(x => x.Name == request.Name && x.Id != id);

        if (NameExisits)
            return Result.Failure(RoleErrors.DuplicatedRole);


        var allowedPermissions = Permissions.GetAllPermissions();

        if (request.Permissions.Except(allowedPermissions).Any())
            return Result.Failure<RoleDetailResponse>(RoleErrors.InvalidPermissions);

        role.Name = request.Name;
        var result = await _roleManager.UpdateAsync(role);

        if (result.Succeeded)
        {
            var currentpermissions = await _context.RoleClaims
                .Where(x => x.RoleId == id && x.ClaimType == Permissions.Type)
                .Select(x => x.ClaimValue)
                .ToListAsync();

            var NewPermissions = request.Permissions.Except(currentpermissions)
                .Select(x => new IdentityRoleClaim<string>
                {
                    ClaimType = Permissions.Type,
                    ClaimValue = x,
                    RoleId = role.Id
                });

            var removedPermissions = currentpermissions.Except(request.Permissions);

            await _context.RoleClaims
                    .Where(x => x.RoleId == id && removedPermissions.Contains(x.ClaimValue))
                    .ExecuteDeleteAsync();

            await _context.RoleClaims.AddRangeAsync(NewPermissions);

            await _context.SaveChangesAsync();

            return Result.Success();

        }
        var error = result.Errors.First();

        return Result.Failure<RoleDetailResponse>(new
            Error(error.Code, error.Description, StatusCodes.Status400BadRequest));

    }
    public async Task<Result> ToggleAsync(string id)
    {
        if (await _roleManager.FindByIdAsync(id) is not { } role)
            return Result.Failure(RoleErrors.RoleNotFound);

        role.IsDeleted = !role.IsDeleted;
        await _context.SaveChangesAsync();

        return Result.Success();
    }
}
