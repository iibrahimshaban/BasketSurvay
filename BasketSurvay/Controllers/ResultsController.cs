namespace BasketSurvay.Controllers
{
    [Route("api/Polls/{PollId}/[controller]")]
    [ApiController]
    [HasPermission(Permissions.Results)]
    public class ResultsController(IResultService resultService) : ControllerBase
    {
        private readonly IResultService _resultService = resultService;

        [HttpGet("row-data")]
        public async Task<IActionResult> PollVote([FromRoute] int PollId, CancellationToken cancellationToken = default)
        {
            var result = await _resultService.GetPollVoteAsync(PollId, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : result.Error.ToProblem();
        }
        [HttpGet("votes-per-day")]
        public async Task<IActionResult> VotesPerDay([FromRoute] int PollId, CancellationToken cancellationToken)
        {
            var result = await _resultService.GetVotePerDayAsync(PollId, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : result.Error.ToProblem();
        }
        [HttpGet("votes-per-question")]
        public async Task<IActionResult> VotesPerQuestion([FromRoute] int PollId, CancellationToken cancellationToken)
        {
            var result = await _resultService.GetVotePerQuestionAsync(PollId, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : result.Error.ToProblem();
        }
    }
}
