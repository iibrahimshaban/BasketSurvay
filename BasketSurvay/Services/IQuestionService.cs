
using BasketSurvay.Contracts.Common;

namespace BasketSurvay.Services
{
    public interface IQuestionService
    {
        Task<Result<PaginationList<QuestionResponse>>> GetAllAsync(int PollId, RequestFilter requestFilter, CancellationToken cancellationToken);
        Task<Result<IEnumerable<QuestionResponse>>> GetAvailableAsync(int PollId, string UserId, CancellationToken cancellationToken);
        Task<Result<QuestionResponse>> GetByQuestionIdAsync(int PollId, int QuestionId, CancellationToken cancellationToken);
        Task<Result<QuestionResponse>> CreateAsync(int PollId, QuestionRequest request,
            CancellationToken cancellationToken = default);
        Task<Result<QuestionResponse>> UpdateAsync(int PollId, int id, QuestionRequest request, CancellationToken cancellationToken = default);
        Task<Result<bool>> ToggleStatusAsync(int PollId, int id, CancellationToken cancellationToken = default);
    }
}
