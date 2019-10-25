using System;
using System.Collections.Generic;
using System.Text;
using AnimeFeedManager.Application.Notifications;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AnimeFeedManager.Functions.Extensions
{
    internal static class SendGridMessageExtensions
    {
        internal static SendGridMessage AddInfoFromNotification(this SendGridMessage @this, Notification notification)
        {
            @this.AddTo(notification.Subscriber);
            @this.SetSubject(DefaultSubject());
            @this.AddContent(MimeType.Html, CreateHtmlBody(notification.Feeds));
            return @this;
        }

        private static string CreateHtmlBody(IEnumerable<SubscribedFeed> feeds)
        {
            const string tableStyle = "style=\"border: 1px solid black;border-collapse:collapse;\"";
            const string rowStyle = "style=\"border: 1px solid black;vertical-align:top;\"";
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<p>Available download from your subscription for today:</p>");
            stringBuilder.AppendLine($"<table {tableStyle}");
            stringBuilder.AppendLine($"<tr><th width=\"40%\" {rowStyle}>Title</th><th width=\"60%\" {rowStyle}>Magnet</th></tr>");
            foreach (var subscribedFeed in feeds)
            {
                stringBuilder.AppendLine("<tr>");
                stringBuilder.AppendLine($"<td {rowStyle}><strong>{subscribedFeed.Title}</strong></td>");
                stringBuilder.AppendLine($"<td {rowStyle}>{LinkFromFeed(subscribedFeed)}</td>");
                stringBuilder.AppendLine("</tr>");
            }
            stringBuilder.AppendLine("</table>");

            return stringBuilder.ToString();
        }

        private static string LinkFromFeed(SubscribedFeed feed) => $"<a href=\"{feed.Link}\" title=\"{feed.Title} Magnet\" target=\"_blank\">{feed.Link}</a>";

        private static string DefaultSubject() => $"Subscriptions Available for Download ({DateTime.Today.ToShortDateString()})";
    }
}
