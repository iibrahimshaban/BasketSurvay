using BasketSurvay.Contracts.Votes;

namespace BasketSurvay.Services
{
    public interface IVoteService
    {
        Task<Result> AddVoteAsync(int PollId, string UserId, VoteRequest request
            , CancellationToken cancellationToken = default);
    }
}
