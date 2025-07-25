namespace BasketSurvay.Abstractions.Consts;

public static class DefaultRoles
{
    public partial class Admin
    {
        public const string Name = nameof(Admin);
        public const string Id = "92b75286-d8f8-4061-9995-e6e23ccdee94";
        public const string ConcurrencyStamp = "f51e5a91-bced-49c2-8b86-c2e170c0846c";
    }
    public partial class Member
    {
        public const string Name = nameof(Member);
        public const string Id = "9eaa03df-8e4f-4161-85de-0f6e5e30bfd4";
        public const string ConcurrencyStamp = "5ee6bc12-5cb0-4304-91e7-6a00744e042a";
    }
}
