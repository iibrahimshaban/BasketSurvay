namespace BasketSurvay.Abstractions.Consts;

public static class Permissions
{
    public static string Type { get; } = "Permissions";

    public const string GetPolls = "Polls:Read";
    public const string CreatePolls = "Polls:Create";
    public const string UpdatePolls = "Polls:Update";
    public const string DeletePolls = "Polls:Delete";

    public const string GetQuestions = "Questions:Read";
    public const string CreateQuestions = "Questions:Create";
    public const string UpdateQuestions = "Questions:Update";

    public const string GetUsers = "User:Read";
    public const string CreateUsers = "User:create";
    public const string UpdateUsers = "User:Update";

    public const string GetRoles = "Roles:Read";
    public const string CreateRoles = "Roles:create";
    public const string UpdateRoles = "Roles:Update";

    public const string Results = "results:Read";

    public static IList<string?> GetAllPermissions() =>
        typeof(Permissions).GetFields().Select(field => field.GetValue(field) as string).ToList();

}
