﻿using System.Diagnostics.Contracts;
using System.Text;

namespace AnimeFeedManager.Features.Domain.Errors
{
    public sealed class ExceptionError : DomainError
    {
        public Exception Exception { get; }

        private ExceptionError(Exception exn, string correlationId) : base(correlationId,  exn.Message)
        {
            Exception = exn;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var innerErrors = ExtractErrors(Exception);
            builder.AppendLine($"[{CorrelationId}] - {Message}");
            if (!innerErrors.Any()) return builder.ToString();
            builder.AppendLine("Inner Errors: ");
            foreach (var error in innerErrors)
            {
                builder.AppendLine($"{error}");
            }

            return builder.ToString();
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

        public static ExceptionError FromException(Exception exn, string correlationId) => new(exn, correlationId);
        public static ExceptionError FromException(Exception exn) => new(exn, "Generic_Exception");
    }
}