
using BasketSurvay.Contracts.Common;

namespace BasketSurvay.Controllers
{
    [Route("api/Polls/{PollId}/[controller]")]
    [ApiController]
    public class QuestionsController(IQuestionService questionService) : ControllerBase
    {
        private readonly IQuestionService _questionService = questionService;

        [HttpGet("all")]
        [HasPermission(Permissions.GetQuestions)]
        public async Task<IActionResult> GetAll([FromRoute] int PollId, [FromQuery] RequestFilter requestFilter, CancellationToken cancellationToken)
        {
            var result = await _questionService.GetAllAsync(PollId, requestFilter, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : result.Error.ToProblem();
        }

        [HttpGet("{id}")]
        [HasPermission(Permissions.GetQuestions)]
        public async Task<IActionResult> Get([FromRoute] int PollId, [FromRoute] int id, CancellationToken cancellationToken)
        {
            var result = await _questionService.GetByQuestionIdAsync(PollId, id, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : result.Error.ToProblem();

        }
        [HttpPost("")]
        [HasPermission(Permissions.CreateQuestions)]
        public async Task<IActionResult> Create([FromRoute] int PollId, [FromBody] QuestionRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _questionService.CreateAsync(PollId, request, cancellationToken);

            return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { PollId, result.Value.Id }, result.Value)
            : result.Error.ToProblem();
        }
        [HttpPut("{id}")]
        [HasPermission(Permissions.UpdateQuestions)]
        public async Task<IActionResult> Update([FromRoute] int pollId, [FromRoute] int id, QuestionRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _questionService.UpdateAsync(pollId, id, request, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : result.Error.ToProblem();
        }
        [HttpPut("{id}/toggle-status")]
        [HasPermission(Permissions.UpdateQuestions)]
        public async Task<IActionResult> ToggleStatus([FromRoute] int PollId, [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            var Result = await _questionService.ToggleStatusAsync(PollId, id, cancellationToken);

            if (Result.IsFailur)
                return Result.Error.ToProblem();

            return Ok(Result.Value);
        }
    }
}
