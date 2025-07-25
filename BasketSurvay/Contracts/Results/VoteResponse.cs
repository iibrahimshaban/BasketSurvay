

namespace BasketSurvay.Contracts
{
    public record VoteResponse(
        string VoterName,
        DateTime VoteDate,
        IEnumerable<QuestionAnswerResponse> SelectedAnswers
        );

}
