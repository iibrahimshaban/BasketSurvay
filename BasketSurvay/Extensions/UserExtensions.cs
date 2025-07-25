using System.Security.Claims;

namespace BasketSurvay.Extensions
{
    public static class UserExtensions
    {
        public static string? GetUserId(this ClaimsPrincipal User)
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);

        }
    }
}
