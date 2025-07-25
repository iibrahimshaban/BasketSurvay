namespace BasketSurvay.Errors;

public static class FiltersErrors
{
    public static readonly Error InvalidPageSize =
    new("Filters.InvalidPageSize", "Please use a page size less than or equal 20 items", StatusCodes.Status404NotFound);
}
