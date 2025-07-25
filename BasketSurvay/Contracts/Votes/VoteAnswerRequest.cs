namespace BasketSurvay.Contracts.Votes
{
    public record VoteAnswerRequest(
        int QuestionId,
        int AnswerId
        );

}
