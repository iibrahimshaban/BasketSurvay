
using BasketSurvay.Services;

namespace BasketSurvay.Controllers;

[Route("api/[controller]")] // api/polls
[ApiController]
public class PollsController(IPollServices pollServices) : ControllerBase
{
    private readonly IPollServices _pollServices = pollServices;

    [HttpGet("All")]
    public IActionResult GetAll()
    {
        return Ok(_pollServices.GetAll());
    }
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var poll = _pollServices.GetById(id);

        return poll is null ? NotFound("no poll with this id ") : Ok(poll);
    }
    [HttpPost("")] // to make an empty route
    public IActionResult Create(Poll request)
    {
        var NewPoll = _pollServices.Create(request);

        return CreatedAtAction(nameof(Get), new { id = NewPoll.Id }, NewPoll);
    }
    [HttpPut("{id}")]
    public IActionResult Update(int id ,Poll request)
    {
        request.Id = id;
        bool status = _pollServices.Update(request);

        return status ? NoContent() : BadRequest("invalid update process check your values ");
    }
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
       bool IsDeleted = _pollServices.Delete(id);
        return IsDeleted ? NoContent() : BadRequest("vaild to delete ");
    }


}
