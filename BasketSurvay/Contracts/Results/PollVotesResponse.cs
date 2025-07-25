namespace BasketSurvay.Contracts.Results
{
    public record PollVotesResponse(
        string Title,
        IEnumerable<VoteResponse> Votes
        );

}
