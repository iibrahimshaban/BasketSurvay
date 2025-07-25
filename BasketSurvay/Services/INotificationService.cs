namespace BasketSurvay.Services;

public interface INotificationService
{
    Task SendNewPollNotification(int? PollId = null);
}
