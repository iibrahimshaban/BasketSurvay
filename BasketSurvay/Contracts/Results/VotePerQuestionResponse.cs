namespace BasketSurvay.Contracts.Results
{
    public record VotePerQuestionResponse(
        string Question,
        IEnumerable<VotesPerAnswerResponse> Answers
        );

}
