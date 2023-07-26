using Fanex.MailDeliver.Model;
using Microsoft.AspNetCore.Hosting;
using RazorEngine;
using RazorEngine.Templating;

namespace ThirdPartyBroadcast.Services;

public interface SendMailService
{
    void SendMail(NotificationModel item);
}

public class SendMailServiceImp : SendMailService
{
    private readonly IHostingEnvironment environment;

    public SendMailServiceImp(IHostingEnvironment environment)
    {
        this.environment = environment;
    }

    public void SendMail(NotificationModel item)
    {
        var mailRecord = new MailRecord
        {
            MailTo = item.To,
            MailCc = item.Copy,
            Title = "NAP - Settlement System",
            Subject = Subject(item),
            Body = MailBody(item)
        };

        using var mailClient = new Fanex.MailDeliver.MailClient.API.MailDeliver();

        mailClient.SendMail(mailRecord);
    }

    private static string Subject(NotificationModel item)
    {
        return $"[Virtual Sports] {item.Action} Ticket Numbers (Licensee) -- Resettle MatchID: {item.MatchId} ({item.MatchInfo.SportName})";
    }

    private string MailBody(NotificationModel item)
    {
        var filePath = environment.WebRootPath;
        string templateKey;
        string template;

        if (item.Type == EmailType.Credit)
        {
            templateKey = "credit-key";
            template = $"{Path.Combine(filePath, "email-templates", "credit-template.html")}";
        }
        else
        {
            templateKey = "deposit-key";
            template = $"{Path.Combine(filePath, "email-templates", "deposit-template.html")}";
        }
        return Engine.Razor.RunCompile(File.ReadAllText(template), templateKey, null, item);
    }
}