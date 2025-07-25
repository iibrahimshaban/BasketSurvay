
using BasketSurvay.Contracts.Votes;
using Microsoft.AspNetCore.RateLimiting;
namespace BasketSurvay.Controllers
{
    [Route("api/Polls/{PollId}/Vote")]
    [ApiController]
    [Authorize(Roles = DefaultRoles.Member.Name)]
    [EnableRateLimiting("concurrency")]
    public class VotesController(IQuestionService questionService, IVoteService voteService) : ControllerBase
    {
        private readonly IQuestionService _questionService = questionService;
        private readonly IVoteService _voteService = voteService;

        [HttpGet("")]
        public async Task<IActionResult> Start([FromRoute] int PollId, CancellationToken cancellationToken)
        {
            var UserId = User.GetUserId();
            var result = await _questionService.GetAvailableAsync(PollId, UserId!, cancellationToken);

            if (result.IsSuccess)
                return Ok(result.Value);

            return result.IsSuccess
                ? Ok(result.Value)
                : result.Error.ToProblem();
        }
        [HttpPost("")]
        public async Task<IActionResult> Vote([FromRoute] int PollId, [FromBody] VoteRequest request
            , CancellationToken cancellationToken)
        {
            var UserId = User.GetUserId();
            var result = await _voteService.AddVoteAsync(PollId, UserId!, request, cancellationToken);

            return result.IsSuccess
                ? Created()
                : result.Error.ToProblem();
        }
    }
}
