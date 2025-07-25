using BasketSurvay.Contracts.Users;

namespace BasketSurvay.Controllers;
[Route("api/[controller]")]
[ApiController]
public class UsersController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet("")]
    [HasPermission(Permissions.GetUsers)]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllUsersAsync(cancellationToken);

        return Ok(users);
    }

    [HttpGet("members")]
    [HasPermission(Permissions.GetUsers)]
    public async Task<IActionResult> GetAllMembers(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllMembersAsync(cancellationToken);

        return Ok(users);
    }
    [HttpGet("{id}")]
    [HasPermission(Permissions.GetUsers)]
    public async Task<IActionResult> GetById([FromRoute] string id, CancellationToken cancellationToken)
    {
        var result = await _userService.GetByIdAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.Error.ToProblem();
    }
    [HttpPost("")]
    [HasPermission(Permissions.CreateUsers)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _userService.CreateAsync(request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : result.Error.ToProblem();
    }
    [HttpPut("{UserId}")]
    [HasPermission(Permissions.UpdateUsers)]
    public async Task<IActionResult> Update([FromRoute] string UserId, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _userService.UpdateUserAsync(UserId, request, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.ToProblem();
    }
    [HttpPut("{UserId}/toggle-status")]
    [HasPermission(Permissions.UpdateUsers)]
    public async Task<IActionResult> ToggleUser([FromRoute] string UserId, CancellationToken cancellationToken)
    {
        var result = await _userService.ToggleStatus(UserId, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.ToProblem();
    }
    [HttpPut("{UserId}/unlock")]
    [HasPermission(Permissions.UpdateUsers)]
    public async Task<IActionResult> UnLockUser([FromRoute] string UserId, CancellationToken cancellationToken)
    {
        var result = await _userService.UnlockAsync(UserId, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.ToProblem();
    }
}
