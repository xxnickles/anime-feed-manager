﻿namespace AnimeFeedManager.Features.Common.Error;

public sealed class NoContentError : DomainError
{
    private NoContentError(string correlationId, string message) : base(correlationId, message)
    {
    }

    public static NoContentError Create(string correlationId, string message) => new(correlationId, message);
}