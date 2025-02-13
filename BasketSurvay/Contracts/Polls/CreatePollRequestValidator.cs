using System.Reflection.Metadata.Ecma335;

namespace BasketSurvay.Contracts.Polls
{
    public class CreatePollRequestValidator : AbstractValidator<CreatePollRequest>
    {
        public CreatePollRequestValidator()
        {
            //title is required & at least to be 4 char & at max to be 10 char
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("add a title") //write your own message 
                .Length(4, 100);

            RuleFor(x => x.Summary)
                .NotEmpty().WithMessage("add {PropertyName}")
                .Length(4, 1000);

            RuleFor(x => x.StartsAt)
                .NotEmpty()
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today));

            RuleFor(x => x.EndsAt)
                .NotEmpty();

            RuleFor(x => x)
                .Must(HasValidDate)
                .WithName(nameof(CreatePollRequest.EndsAt))
                .WithMessage("{PropertyName} must be greater than or equal start date ");
        }
        public bool HasValidDate(CreatePollRequest request) =>
            request.EndsAt >= request.StartsAt;
    }
}
