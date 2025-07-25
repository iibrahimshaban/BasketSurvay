namespace BasketSurvay.Errors
{
    public static class PollErrors
    {
        public static readonly Error PollNotFound =
            new("Poll.NotFound", "can't find poll with given id", StatusCodes.Status404NotFound);
        public static readonly Error DuplicatedPoll =
            new("Poll.Duplicated", "there is already an poll with the same information", StatusCodes.Status409Conflict);
        public static readonly Error NoPoll =
            new("Polls.Empty", "there is not any polls ", StatusCodes.Status404NotFound);
    }
}
