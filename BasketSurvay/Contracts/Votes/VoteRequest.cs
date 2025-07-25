namespace BasketSurvay.Contracts.Votes
{
    public record VoteRequest(
        IEnumerable<VoteAnswerRequest> Answers
        );

}
