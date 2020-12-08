using OwnID.Extensibility.Flow;

namespace OwnID.Extensibility.Metrics
{
    /// <summary>
    ///     DO NOT RENAME IT!
    /// </summary>
    public enum EventType
    {
        Unknown,
        Login,
        Register,
        Link,
        Recover
    }

    // TODO: add check to startup for new eventTypes
    public static class EventTypeConverterExtensions
    {
        public static EventType ToEventType(this ChallengeType challengeType)
        {
            return challengeType switch
            {
                ChallengeType.Register => EventType.Register,
                ChallengeType.Login => EventType.Login,
                ChallengeType.Link => EventType.Link,
                ChallengeType.Recover => EventType.Recover,
                _ => EventType.Unknown
            };
        }
    }
}