namespace BasketSurvay.Abstractions
{
    public static class ResultExtensions
    {
        public static ObjectResult ToProblem(this Error error)
        {

            var Problem = Results.Problem(statusCode: error.StatusCode);
            var problemDetails = Problem.GetType().GetProperty(nameof(ProblemDetails))!.GetValue(Problem) as ProblemDetails;

            problemDetails!.Extensions = new Dictionary<string, object?>
            {
                 {
                    "Errors" , new []
                    {
                        error.Code,
                        error.Description
                    }
                 }
            };

            return new ObjectResult(problemDetails);
        }
    }
}
