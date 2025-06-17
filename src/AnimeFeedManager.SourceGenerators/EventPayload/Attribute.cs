using System;

namespace AnimeFeedManager.SourceGenerators.EventPayload
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class EventPayloadSerializerContextAttribute(Type payloadType) : Attribute
    {
        public Type PayloadType { get; } = payloadType;
    }

}