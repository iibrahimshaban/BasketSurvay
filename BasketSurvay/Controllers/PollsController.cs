
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BasketSurvay.Controllers;

[Route("api/[controller]")] // api/polls
[ApiController]
public class PollsController(IPollServices pollServices) : ControllerBase
{
    private readonly IPollServices _pollServices = pollServices;

    [HttpGet("All")]
    public IActionResult GetAll()
    {
        var Polls = _pollServices.GetAll();
          
        var Response = Polls.Adapt<IEnumerable<PollResponse>>();

        return Ok(Response);
    }
    [HttpGet("{id}")]
    public IActionResult Get([FromRoute]int id)
    {
        var poll = _pollServices.GetById(id);
        if (poll is null)
            return NotFound();

        var response = poll.Adapt<PollResponse>();

        return Ok(response);
    }
    [HttpPost("")] // to make an empty route
    public IActionResult Create([FromBody]CreatePollRequest request)
    {

        var NewPoll = _pollServices.Create(request.Adapt<Poll>());

        return CreatedAtAction(nameof(Get), new { id = NewPoll.Id }, NewPoll.Adapt<PollResponse>());
    }
    [HttpPut("{id}")]
    public IActionResult Update([FromRoute]int id ,[FromBody]CreatePollRequest request)
    {
        
        bool status = _pollServices.Update(id,request.Adapt<Poll>());

        return status ? NoContent() : BadRequest("invalid update process check your values ");
    }
    [HttpDelete("{id}")]
    public IActionResult Delete([FromRoute]int id)
    {
       bool IsDeleted = _pollServices.Delete(id);
        return IsDeleted ? NoContent() : BadRequest("vaild to delete ");
    }
    


}
