namespace BasketSurvay.Abstractions.Consts
{
    public static class RegexPattern
    {
        public const string Password = "^(?=.*[0-9])(?=.*[!@#$%^&*()\\-_=+{};:,<.>/?])(?=.*[a-z])(?=.*[A-Z]).{8,}$";
        public const string UserName = "^[a-zA-Z][a-zA-Z0-9_]{2,15}$";

    }
}
