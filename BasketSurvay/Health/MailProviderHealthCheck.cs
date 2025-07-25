using BasketSurvay.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BasketSurvay.Health; // place in code

public class MailProviderHealthCheck(IOptions<MailSettings> options) : IHealthCheck
{
    private readonly MailSettings _mailSettings = options.Value;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {

            using var smtp = new SmtpClient();

            smtp.CheckCertificateRevocation = false;
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password, cancellationToken);

            return Task.FromResult(HealthCheckResult.Healthy("healthy"));

        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(exception: ex));
        }
    }
}
