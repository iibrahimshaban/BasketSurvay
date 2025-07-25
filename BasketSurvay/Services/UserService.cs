using BasketSurvay.Contracts.Users;
namespace BasketSurvay.Services;

public class UserService(UserManager<ApplicationUser> userManger, ApplicationDbContext context) : IUserService
{
    private readonly UserManager<ApplicationUser> _userManger = userManger;
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<UserResponse>> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        var allUsers = await (from user in _context.Users
                              join userRole in _context.UserRoles
                              on user.Id equals userRole.UserId
                              join role in _context.Roles
                              on userRole.RoleId equals role.Id into roles
                              where roles.All(x => x.Name != DefaultRoles.Member.Name)
                              select new
                              {
                                  user.Id,
                                  user.UserName,
                                  user.FirstName,
                                  user.LastName,
                                  user.Email,
                                  user.IsDisabled,
                                  roles = roles.Select(x => x.Name!).ToList()
                              }
                              )
                              .GroupBy(u => new { u.Id, u.UserName, u.FirstName, u.LastName, u.Email, u.IsDisabled })
                              .Select(x => new UserResponse
                              (
                                 x.Key.Id,
                                 x.Key.UserName!,
                                 x.Key.FirstName,
                                 x.Key.LastName,
                                 x.Key.Email,
                                 x.Key.IsDisabled,
                                 x.SelectMany(r => r.roles)
                              ))
                             .ToListAsync(cancellationToken);

        return allUsers;
    }
    public async Task<IEnumerable<UserResponse>> GetAllMembersAsync(CancellationToken cancellationToken)
    {
        var Members = await (from user in _context.Users
                             join userRole in _context.UserRoles
                             on user.Id equals userRole.UserId
                             join role in _context.Roles
                             on userRole.RoleId equals role.Id into roles
                             where roles.All(x => x.Name == DefaultRoles.Member.Name)
                             select new UserResponse
                             (
                                 user.Id,
                                 user.UserName!,
                                 user.FirstName,
                                 user.LastName,
                                 user.Email!,
                                 user.IsDisabled,
                                 roles.Select(x => x.Name!).ToList()
                             ))
                              .ToListAsync(cancellationToken);

        return Members;
    }
    public async Task<Result<UserResponse>> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (await _userManger.FindByIdAsync(userId) is not { } user)
            return Result.Failure<UserResponse>(UserErrors.UserNotFound);

        var userRoles = await _userManger.GetRolesAsync(user);

        var response = (user, userRoles).Adapt<UserResponse>();

        return Result.Success(response);
    }
    public async Task<Result<UserResponse>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var UserIsExists = await _userManger.Users.AnyAsync(x => x.Email == request.Email, cancellationToken);

        if (UserIsExists)
            return Result.Failure<UserResponse>(UserErrors.DuplicatedEmail);

        var UserNameIsExists = await _userManger.Users.AnyAsync(x => x.UserName == request.UserName, cancellationToken);

        if (UserIsExists)
            return Result.Failure<UserResponse>(UserErrors.DuplicatedUsername);

        var existingRoles = await _context.Roles
                        .Where(x => !x.IsDeleted)
                        .Select(x => x.Name.ToString())
                        .AsNoTracking()
                        .ToListAsync(cancellationToken);

        if (request.Roles.Except(existingRoles).Any())
            return Result.Failure<UserResponse>(UserErrors.InvalidRoles);

        var user = request.Adapt<ApplicationUser>();

        user.EmailConfirmed = true;

        var result = await _userManger.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            await _userManger.AddToRolesAsync(user, request.Roles);

            var response = (user, request.Roles).Adapt<UserResponse>();

            return Result.Success(response);

        }
        var error = result.Errors.First();

        return Result.Failure<UserResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }
    public async Task<Result> UpdateUserAsync(string UserId, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        if (await _userManger.FindByIdAsync(UserId) is not { } user)
            return Result.Failure(UserErrors.UserNotFound);

        var UserIsExists = await _userManger.Users
            .AnyAsync(x => x.Email == request.Email && x.Id != UserId, cancellationToken);

        if (UserIsExists)
            return Result.Failure<UserResponse>(UserErrors.DuplicatedEmail);

        var UserNameIsExists = await _userManger.Users
            .AnyAsync(x => x.UserName == request.UserName && x.Id != UserId, cancellationToken);

        if (UserIsExists)
            return Result.Failure<UserResponse>(UserErrors.DuplicatedUsername);

        var existingRoles = await _context.Roles
                        .Where(x => !x.IsDeleted)
                        .Select(x => x.Name.ToString())
                        .AsNoTracking()
                        .ToListAsync(cancellationToken);

        if (request.Roles.Except(existingRoles).Any())
            return Result.Failure<UserResponse>(UserErrors.InvalidRoles);

        user = request.Adapt(user);

        var result = await _userManger.UpdateAsync(user);

        if (result.Succeeded)
        {
            await _context.UserRoles
                .Where(x => x.UserId == user.Id)
                .ExecuteDeleteAsync(cancellationToken);

            await _userManger.AddToRolesAsync(user, request.Roles);

            return Result.Success();

        }
        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));

    }
    public async Task<Result> ToggleStatus(string userId, CancellationToken cancellationToken)
    {
        if (await _userManger.FindByIdAsync(userId) is not { } user)
            return Result.Failure(UserErrors.UserNotFound);

        user.IsDisabled = !user.IsDisabled;
        await _userManger.UpdateAsync(user);
        return Result.Success();
    }
    public async Task<Result> UnlockAsync(string userId, CancellationToken cancellationToken)
    {
        if (await _userManger.FindByIdAsync(userId) is not { } user)
            return Result.Failure(UserErrors.UserNotFound);

        await _userManger.SetLockoutEndDateAsync(user, DateTime.UtcNow);

        return Result.Success();
    }
    public async Task<Result<UserProfileResponse>> GetProfileAsync(string UserId)
    {
        var profile = await _userManger.Users
            .Where(x => x.Id == UserId)
            .ProjectToType<UserProfileResponse>()
            .SingleAsync();

        return Result.Success(profile);
    }
    public async Task<Result> UpdateProfileAsync(string UserId, UpdateProfileRequest request)
    {
        var userNameExists = await _userManger.Users
            .AnyAsync(x => x.UserName == request.UserName && x.Id != UserId);

        if (userNameExists)
            return Result.Failure(UserErrors.DuplicatedUsername);

        await _userManger.Users
             .Where(x => x.Id == UserId)
             .ExecuteUpdateAsync(setters =>
                  setters
                     .SetProperty(u => u.UserName, request.UserName)
                     .SetProperty(u => u.FirstName, request.FirstName)
                     .SetProperty(u => u.LastName, request.LastName)
             );

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(string UserId, ChangePasswordRequest request)
    {
        var user = await _userManger.FindByIdAsync(UserId);

        var result = await _userManger.ChangePasswordAsync(user, request.Currentpassword, request.Newpassword);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));

    }
}
