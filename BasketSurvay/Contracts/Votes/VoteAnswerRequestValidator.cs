namespace BasketSurvay.Contracts.Votes
{
    public class VoteAnswerRequestValidator : AbstractValidator<VoteAnswerRequest>
    {
        public VoteAnswerRequestValidator()
        {
            RuleFor(x => x.QuestionId)
                .GreaterThanOrEqualTo(1);

            RuleFor(x => x.AnswerId)
                .GreaterThanOrEqualTo(1);
        }
    }
}
