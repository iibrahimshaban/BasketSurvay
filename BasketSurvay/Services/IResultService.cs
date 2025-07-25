namespace BasketSurvay.Services
{
    public interface IResultService
    {
        Task<Result<PollVotesResponse>> GetPollVoteAsync(int PollId, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<VotesPerDayResponse>>> GetVotePerDayAsync(int PollId, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<VotePerQuestionResponse>>> GetVotePerQuestionAsync(int PollId, CancellationToken cancellationToken = default);
    }
}
