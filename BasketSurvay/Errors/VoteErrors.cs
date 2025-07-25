namespace BasketSurvay.Errors
{
    public static class VoteErrors
    {
        public static readonly Error Existis = new
            Error("vote.AlreadyExistis", "the user had vote on this poll before ", StatusCodes.Status409Conflict);
        public static readonly Error Invalid = new
            Error("Vote.InvalidQuestions", "Questions had been mainapulated", StatusCodes.Status400BadRequest);
    }
}
