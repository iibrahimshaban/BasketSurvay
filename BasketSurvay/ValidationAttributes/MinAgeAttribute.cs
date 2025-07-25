namespace BasketSurvay.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MinAgeAttribute(int MinimumAge) : ValidationAttribute
    {
        private readonly int _minimumAge = MinimumAge;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not null)
            {
                var BirthDate = (DateTime)value;

                if (DateTime.Today < BirthDate.AddYears(_minimumAge))
                    return new ValidationResult(
                        $"invalid {validationContext.DisplayName} ,you are smaller than {_minimumAge} years old");
            }
            return ValidationResult.Success;
        }
    }
}
