using System.Diagnostics.Contracts;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Common.Domain.Errors;

public sealed class ExceptionError : DomainError
{
    public Exception Exception { get; }

    public string? AdditionalMessage { get; }
    private ExceptionError(Exception exn) : base(exn.Message)
    {
        Exception = exn;
    }
    
    private ExceptionError(Exception exn, string additionalMessage) : base(exn.Message)
    {
        Exception = exn;
        AdditionalMessage = additionalMessage;
    }


    public override string ToString()
    {
        var builder = new StringBuilder();
        var innerErrors = ExtractErrors(Exception);
        builder.AppendLine(Message);
        if (AdditionalMessage != null)
        {
            builder.AppendLine(AdditionalMessage);
        }
      
        if (!innerErrors.Any()) return builder.ToString();
        builder.AppendLine("Inner Errors: ");
        foreach (var error in innerErrors)
        {
            builder.AppendLine($"{error}");
        }

        return builder.ToString();
    }

    public override void LogError(ILogger logger)
    {
        logger.LogError(Exception, "{Error} {Additional}", Exception.Message, AdditionalMessage);
    }

    [Pure]
    private static IImmutableList<string> ExtractErrors(Exception exn)
    {
        var lst = ImmutableList<string>.Empty;
        lst = lst.Add(exn.Message);
        if (exn.InnerException != null)
            lst = lst.AddRange(ExtractErrors(exn.InnerException));
        return lst;
    }

    public static ExceptionError FromException(Exception exn) => new(exn);
    public static ExceptionError FromException(Exception exn, string additionalMessage) => new(exn, additionalMessage);
}