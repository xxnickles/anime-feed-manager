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
            const string tableStyle = "style=\"max-width: 900px;  margin: auto;\"";
            const string rowStyle = "style=\"vertical-align:top; padding: 20px 15px;\"";
            const string dateStyle = "style=\"font-size: 12px; line-height: 1.5; margin: 0;\"";
            const string contentStyle = "style=\"line-height: 1.2; font-size: 18px; color: #2bbbb2; margin: 0;\"";
            const string linkStyle = "style=\"margin: 0;\"";
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(
                "<div style=\"font-family: Arial, 'Helvetica Neue', Helvetica, sans-serif; background-color:#f6f6f4;\">");
            stringBuilder.AppendLine(
                "<p style=\"color: #28404F\">Available download links based on your subscription for Today:</p>");
            stringBuilder.AppendLine($"<table {tableStyle}");
            stringBuilder.AppendLine("<tbody>");
            foreach (var subscribedFeed in feeds)
            {
                stringBuilder.AppendLine("<tr>");
                stringBuilder.AppendLine($"<td {rowStyle}>");
                stringBuilder.AppendLine($"<p {dateStyle}>{subscribedFeed.PublicationDate:f}</p>");
                stringBuilder.AppendLine($"<p {contentStyle}><strong>{subscribedFeed.Title}</strong></p>");
                stringBuilder.AppendLine($"<p {linkStyle}><a href='{ LinkFromFeed(subscribedFeed)}'>Torrent</a></p>");
                stringBuilder.AppendLine("</td>");
                stringBuilder.AppendLine("</tr>");
            }

            stringBuilder.AppendLine("</tbody>");
            stringBuilder.AppendLine("</table>");
            stringBuilder.AppendLine("</div>");

            return stringBuilder.ToString();
        }

        private static string LinkFromFeed(SubscribedFeed feed)
        {
            return $"<a href=\"{feed.Link}\" title=\"{feed.Title} Magnet\" target=\"_blank\">{feed.Link}</a>";
        }

        private static string DefaultSubject()
        {
            return $"Subscriptions Available for Download ({DateTime.Today.ToShortDateString()})";
        }
    }
}