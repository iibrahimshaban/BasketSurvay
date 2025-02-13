
using BasketSurvay.Contracts.Polls;
using Microsoft.AspNetCore.Authorization;


namespace BasketSurvay.Controllers;

[Route("api/[controller]")] // api/polls
[ApiController]
public class PollsController(IPollServices pollServices) : ControllerBase
{
    private readonly IPollServices _pollServices = pollServices;

    [HttpGet("All")]
    [Authorize]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var Polls = await _pollServices.GetAllAsync(cancellationToken);
          
        var Response = Polls.Adapt<IEnumerable<PollResponse>>();

        return Ok(Response);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute]int id, CancellationToken cancellationToken)
    {
        var poll = await _pollServices.GetByIdAsync(id, cancellationToken);
        if (poll is null)
            return NotFound();

        var response = poll.Adapt<PollResponse>();

        return Ok(response);
    }
    [HttpPost("")] // to make an empty route
    public async Task<IActionResult> Create([FromBody]CreatePollRequest request ,
        CancellationToken cancellationToken)
    {

        var NewPoll =await _pollServices.CreateAsync(request.Adapt<Poll>(), cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = NewPoll.Id }, NewPoll.Adapt<PollResponse>());
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute]int id ,[FromBody]CreatePollRequest request,
        CancellationToken cancellationToken)
    {
        
        bool status = await _pollServices.UpdateAsync(id,request.Adapt<Poll>(), cancellationToken);

        return status ? NoContent() : BadRequest("invalid update process check your values ");
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute]int id , CancellationToken cancellationToken)
    {
       bool IsDeleted = await _pollServices.DeleteAsync(id, cancellationToken);
        return IsDeleted ? NoContent() : BadRequest("vaild to delete ");
    }
    [HttpPut("{id}/TogglePublish")]
    public async Task<IActionResult> TogglePublish([FromRoute] int id,CancellationToken cancellationToken)
    {
       var isUpdated = await _pollServices.TogglePublishStatusAsync(id, cancellationToken);
        if (!isUpdated)
            return NotFound();

        var poll = await _pollServices.GetByIdAsync(id);
        if (poll!.IsPublished)
            return Ok("True");

        return Ok("false");
    }

}
