using BasketSurvay.Contracts.Users;

namespace BasketSurvay.Services;

public interface IUserService
{
    Task<IEnumerable<UserResponse>> GetAllUsersAsync(CancellationToken cancellationToken);
    Task<IEnumerable<UserResponse>> GetAllMembersAsync(CancellationToken cancellationToken);
    Task<Result<UserResponse>> GetByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<UserResponse>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateUserAsync(string UserId, UpdateUserRequest request, CancellationToken cancellationToken);
    Task<Result> ToggleStatus(string userId, CancellationToken cancellationToken);
    Task<Result> UnlockAsync(string userId, CancellationToken cancellationToken);
    Task<Result<UserProfileResponse>> GetProfileAsync(string UserId);
    Task<Result> UpdateProfileAsync(string UserId, UpdateProfileRequest request);
    Task<Result> ChangePasswordAsync(string UserId, ChangePasswordRequest request);
}
