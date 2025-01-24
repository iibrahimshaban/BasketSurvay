
namespace BasketSurvay.Contracts.Validation
{
    public class CreatePollRequestValidator : AbstractValidator<CreatePollRequest>
    {
        public CreatePollRequestValidator()
        {
            //title is required & at least to be 4 char & at max to be 10 char
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("add a title") //write your own message 
                .Length(4, 10);
           
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("add {PropertyName}")
                .Length(4, 100);
        }
    }
}
