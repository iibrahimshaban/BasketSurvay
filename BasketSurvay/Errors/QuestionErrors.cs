namespace BasketSurvay.Errors
{
    public static class QuestionErrors
    {
        public static readonly Error DoublicatedQuestionContent = new(
            "question.DoublicatedContent", "the Question is already exsists in this Poll", StatusCodes.Status409Conflict);
        public static readonly Error NotFound = new(
            "question.NotFound", "no questions founded for this poll", StatusCodes.Status404NotFound);
    }
}
