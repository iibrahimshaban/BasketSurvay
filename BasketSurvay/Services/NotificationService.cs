
using BasketSurvay.Helpers;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BasketSurvay.Services;

public class NotificationService(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor,
    IEmailSender emailSender,
    ILogger<NotificationService> logger) : INotificationService
{
    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ILogger<NotificationService> _logger = logger;

    public async Task SendNewPollNotification(int? PollId = null)
    {
        IEnumerable<Poll> polls = [];

        if (PollId.HasValue)
        {
            var poll = await _context.Polls.SingleOrDefaultAsync(x => x.Id == PollId && x.IsPublished);
            polls = [poll!];
        }
        else
        {
            polls = await _context.Polls
                .Where(x => x.StartsAt == DateOnly.FromDateTime(DateTime.UtcNow) && x.IsPublished)
                .AsNoTracking()
                .ToListAsync();
        }

        var Users = await _userManager.GetUsersInRoleAsync(DefaultRoles.Member.Name);

        var host = _httpContextAccessor.HttpContext?.Request.Headers.Host;
        _logger.LogInformation("host {hh}", host.ToString());

        foreach (var user in Users)
        {
            foreach (var poll in polls)
            {

                var Placeholders = new Dictionary<string, string>
                {
                    {"{{name}}",user.FirstName+" "+user.LastName },
                    {"{{pollTill}}",poll.Title },
                    {"{{endDate}}",poll.EndsAt.ToString() },
                    {"{{url}}",$"https://localhost:7095/api/Polls/{poll.Id}" }
                };

                var body = EmailBodyBuilder.GenerateEmailBody("PollNotification", Placeholders);

                await _emailSender.SendEmailAsync(user.Email!, $"💥 Survay Basket : New Poll - {poll.Title}", body);
            }

        }
    }
}
