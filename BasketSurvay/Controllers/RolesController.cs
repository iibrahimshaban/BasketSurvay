namespace BasketSurvay.Controllers;
[Route("api/[controller]")]
[ApiController]
public class RolesController(IRoleService roleService) : ControllerBase
{
    private readonly IRoleService _roleService = roleService;

    [HttpGet("")]
    [HasPermission(Permissions.GetRoles)]
    public async Task<IActionResult> GetAll([FromQuery] bool? includeDisabled)
    {
        var roles = await _roleService.GetAllAsync(includeDisabled);

        return Ok(roles);
    }
    [HttpGet("{id}")]
    [HasPermission(Permissions.GetRoles)]
    public async Task<IActionResult> GetById([FromRoute] string id)
    {
        var result = await _roleService.GetDetailsAsync(id);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.ToProblem();
    }
    [HttpPost("")]
    [HasPermission(Permissions.CreateRoles)]
    public async Task<IActionResult> Create([FromBody] RoleRequest request)
    {
        var result = await _roleService.CreateAsync(request);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { result.Value.Id }, result.Value)
            : result.Error.ToProblem();
    }
    [HttpPut("{id}")]
    [HasPermission(Permissions.UpdateRoles)]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] RoleRequest request)
    {
        var result = await _roleService.updateAsync(id, request);

        return result.IsSuccess
            ? NoContent()
            : result.Error.ToProblem();
    }
    [HttpPut("{id}/toggle-status")]
    [HasPermission(Permissions.UpdateRoles)]
    public async Task<IActionResult> Toggle([FromRoute] string id)
    {
        var result = await _roleService.ToggleAsync(id);

        return result.IsSuccess
            ? NoContent()
            : result.Error.ToProblem();
    }
}
