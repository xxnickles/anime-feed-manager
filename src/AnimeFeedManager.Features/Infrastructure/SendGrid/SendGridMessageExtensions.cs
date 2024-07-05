using System.Text;
using System.Text.RegularExpressions;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AnimeFeedManager.Features.Infrastructure.SendGrid;

public static partial class SendGridMessageExtensions
{
    // internal static SendGridMessage AddInfoFromNotification(this SendGridMessage @this, ShortSeriesSubscriptionCollection notification, string type)
    // {
    //     @this.AddTo(notification.Subscriber);
    //     @this.SetSubject(DefaultSubject());
    //     @this.AddContent(MimeType.Html, CreateHtmlBody(notification.Series, type));
    //     return @this;
    // }
    
    public static void AddInfoFromNotification(this SendGridMessage @this, SubscriberTvNotification notification)
    {
        @this.AddTo(notification.Subscriber);
        @this.SetSubject(DefaultSubject());
        @this.AddContent(MimeType.Html, CreateHtmlBody(notification.Feeds));
    }

    // private static string CreateHtmlBody(IEnumerable<ShortSeries> feeds, string type)
    // {
    //     const string rowStyle = "style=\"vertical-align:top; padding: 20px 15px;\"";
    //     const string dateStyle = "style=\"font-size: 12px; line-height: 1.5; margin: 0;\"";
    //     const string contentStyle = "style=\"line-height: 1.2; font-size: 18px; color: #2bbbb2; margin: 0;\"";
    //     var stringBuilder = new StringBuilder();
    //     stringBuilder.Append(
    //         @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">");
    //     stringBuilder.Append("<html>");
    //     stringBuilder.Append("<head>");
    //     stringBuilder.Append(@"<style type=""text/css"">
    //                 body, p, div {
    //                   font-family: Arial, 'Helvetica Neue', Helvetica, sans-serif;
    //                   font-size: 16px;
    //                 }
    //                 </style>");
    //     stringBuilder.Append("</head>");
    //     stringBuilder.Append("<body>");
    //     stringBuilder.AppendLine(
    //         $"<p style=\"color: #28404F\">The following {type}s has been released:</p>");
    //     stringBuilder.AppendLine("<table>");
    //     stringBuilder.AppendLine("<tbody>");
    //     foreach (var subscribedFeed in feeds)
    //     {
    //         stringBuilder.AppendLine("<tr>");
    //         stringBuilder.AppendLine($"<td {rowStyle}>");
    //         stringBuilder.AppendLine($"<p {dateStyle}>{subscribedFeed.Publication:f}</p>");
    //         stringBuilder.AppendLine($"<p {contentStyle}><strong>{subscribedFeed.Title}</strong></p>");
    //         stringBuilder.AppendLine("</td>");
    //         stringBuilder.AppendLine("</tr>");
    //     }
    //     stringBuilder.AppendLine("</tbody>");
    //     stringBuilder.AppendLine("</table>");
    //     stringBuilder.Append("</body>");
    //     stringBuilder.Append("</html>");
    //
    //     return stringBuilder.ToString();
    // }

    private static string CreateHtmlBody(IEnumerable<SubscribedFeed> feeds)
    {
        const string rowStyle = "style=\"vertical-align:top; padding: 20px 15px;\"";
        const string dateStyle = "style=\"font-size: 12px; line-height: 1.5; margin: 0;\"";
        const string contentStyle = "style=\"line-height: 1.2; font-size: 18px; color: #2bbbb2; margin: 0;\"";
        const string linkStyle = "style=\"margin: 0;\"";
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(
            """<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">""");
        stringBuilder.Append("<html>");
        stringBuilder.Append("<head>");
        stringBuilder.Append("""
                             <style type="text/css">
                                                 body, p, div {
                                                   font-family: Arial, 'Helvetica Neue', Helvetica, sans-serif;
                                                   font-size: 16px;
                                                 }
                                                 </style>
                             """);
        stringBuilder.Append("</head>");
        stringBuilder.Append("<body>");
        stringBuilder.AppendLine(
            "<p style=\"color: #28404F\">Available download links based on your subscription:</p>");
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
            $"<a href=\"{link.Link}\" title=\"{feed.Title}-{link.Type}\" target=\"_blank\">{SplitWordsByCase(link.Type.ToString())}</a>");
        return string.Join(" | ", listOfLinks);
    }

    private static string DefaultSubject()
    {
        return $"Subscriptions Available for Download ({DateTime.Today.ToShortDateString()})";
    }

    private static string SplitWordsByCase(string str)
    {
        var split = MyRegex().Split(str);
        return string.Join(' ', split);
    }

    [GeneratedRegex("(?<!^)(?=[A-Z])")]
    private static partial Regex MyRegex();
}