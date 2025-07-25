namespace BasketSurvay.Services;

public interface IRoleService
{
    Task<IEnumerable<RoleResponse>> GetAllAsync(bool? includeDisable = false);
    Task<Result<RoleDetailResponse>> GetDetailsAsync(string roleId);
    Task<Result<RoleDetailResponse>> CreateAsync(RoleRequest request);
    Task<Result> updateAsync(string id, RoleRequest request);
    Task<Result> ToggleAsync(string id);
}
