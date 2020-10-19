using AnimeFeedManager.Application.Notifications;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            const string rowStyle = "style=\"vertical-align:top; padding: 20px 15px;\"";
            const string dateStyle = "style=\"font-size: 12px; line-height: 1.5; margin: 0;\"";
            const string contentStyle = "style=\"line-height: 1.2; font-size: 18px; color: #2bbbb2; margin: 0;\"";
            const string linkStyle = "style=\"margin: 0;\"";
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(
                @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">");
            stringBuilder.Append("<html>");
            stringBuilder.Append("<head>");
            stringBuilder.Append(@"<style type=""text/css"">
                    body, p, div {
                      font-family: Arial, 'Helvetica Neue', Helvetica, sans-serif;
                      font-size: 16px;
                    }
                    </style>");
            stringBuilder.Append("</head>");
            stringBuilder.Append("<body>");
            stringBuilder.AppendLine(
                "<p style=\"color: #28404F\">Available download links based on your subscription for Today:</p>");
            stringBuilder.AppendLine("<table>");
            stringBuilder.AppendLine("<tbody>");
            foreach (var subscribedFeed in feeds)
            {
                stringBuilder.AppendLine("<tr>");
                stringBuilder.AppendLine($"<td {rowStyle}>");
                stringBuilder.AppendLine($"<p {dateStyle}>{subscribedFeed.PublicationDate:f}</p>");
                stringBuilder.AppendLine($"<p {contentStyle}><strong>{TitleFromFeed(subscribedFeed)}</strong></p>");
                stringBuilder.AppendLine($"<p {linkStyle}>{LinksFromFeed(subscribedFeed)}</p>");
                stringBuilder.AppendLine("</td>");
                stringBuilder.AppendLine("</tr>");
            }
            stringBuilder.AppendLine("</tbody>");
            stringBuilder.AppendLine("</table>");
            stringBuilder.Append("</body>");
            stringBuilder.Append("</html>");

            return stringBuilder.ToString();
        }

        private static string TitleFromFeed(SubscribedFeed feed)
        {
            var suffix = string.IsNullOrEmpty(feed.EpisodeInfo) ? string.Empty : $" - {feed.EpisodeInfo}";
            return $"{feed.Title}{suffix}";
        }

        private static string LinksFromFeed(SubscribedFeed feed)
        {
            var listOfLinks = feed.Links.Select(link =>
                $"<a href=\"{link.Link}\" title=\"{feed.Title}-{link.Type}\" target=\"_blank\">{link.Type}</a>");
            return string.Join(" | ", listOfLinks);
        }

        private static string DefaultSubject()
        {
            return $"Subscriptions Available for Download ({DateTime.Today.ToShortDateString()})";
        }
    }
}