namespace BasketSurvay.Contracts.Questions
{
    public class QuestionRequestValidator : AbstractValidator<QuestionRequest>
    {
        public QuestionRequestValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty()
                .Length(3, 1000);

            RuleFor(x => x.Answers).NotNull();

            RuleFor(x => x.Answers)
                .NotEmpty().NotNull()
                .Must(x => x.Count > 1)
                .WithMessage("question should have more than one answer")
                .When(x => x.Answers != null);

            RuleFor(x => x.Answers)
                .NotNull()
                .Must(x => x.Distinct().Count() == x.Count)
                .WithMessage("you can't doublicate the answer for the same question ")
                .When(x => x.Answers != null);
        }
    }
}
