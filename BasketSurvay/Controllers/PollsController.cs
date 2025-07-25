using Asp.Versioning;

namespace BasketSurvay.Controllers;
[ApiVersion(1, Deprecated = true)]
[ApiVersion(2)]
[Route("api/[controller]")]
[ApiController]
public class PollsController(IPollServices pollServices) : ControllerBase
{
    private readonly IPollServices _pollServices = pollServices;

    [HttpGet("all")]
    [HasPermission(Permissions.GetPolls)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _pollServices.GetAllAsync(cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.ToProblem();
    }
    [MapToApiVersion(1)]
    [HttpGet("current")]
    [Authorize]
    public async Task<IActionResult> GetCurrentV1(CancellationToken cancellationToken)
    {
        var result = await _pollServices.GetCurrentAsyncV1(cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.ToProblem();
    }
    [MapToApiVersion(2)]
    [HttpGet("current")]
    [Authorize]
    public async Task<IActionResult> GetCurrentV2(CancellationToken cancellationToken)
    {
        var result = await _pollServices.GetCurrentAsyncV2(cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.ToProblem();
    }
    [HttpGet("{id}")]
    [HasPermission(Permissions.GetPolls)]
    public async Task<IActionResult> Get([FromRoute] int id, CancellationToken cancellationToken)
    {
        var poll = await _pollServices.GetByIdAsync(id, cancellationToken);

        return poll.IsSuccess
            ? Ok(poll.Value)
            : poll.Error.ToProblem();
    }
    [HttpPost("")]
    [HasPermission(Permissions.CreatePolls)]
    public async Task<IActionResult> Create([FromBody] CreatePollRequest request,
        CancellationToken cancellationToken)
    {
        var NewPollResult = await _pollServices.CreateAsync(request, cancellationToken);

        return NewPollResult.Match(
            Pollresponse => CreatedAtAction(nameof(Get), new { id = Pollresponse.Id }, Pollresponse),
            error => error.ToProblem()
            );

    }
    [HttpPut("{id}")]
    [HasPermission(Permissions.UpdatePolls)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CreatePollRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _pollServices.UpdateAsync(id, request, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.ToProblem();
    }
    [HttpDelete("{id}")]
    [HasPermission(Permissions.DeletePolls)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var IsDeleted = await _pollServices.DeleteAsync(id, cancellationToken);

        return IsDeleted.IsSuccess
            ? NoContent()
            : IsDeleted.Error.ToProblem();
    }
    [HttpPut("{id}/toggle-publish")]
    [HasPermission(Permissions.UpdatePolls)]
    public async Task<IActionResult> TogglePublish([FromRoute] int id, CancellationToken cancellationToken)
    {
        var isUpdated = await _pollServices.TogglePublishStatusAsync(id, cancellationToken);

        if (isUpdated.IsFailur)
            return isUpdated.Error.ToProblem();

        var poll = await _pollServices.GetByIdAsync(id, cancellationToken);

        if (poll.Value.IsPublished)
            return Ok("True");

        return Ok("false");
    }


}
