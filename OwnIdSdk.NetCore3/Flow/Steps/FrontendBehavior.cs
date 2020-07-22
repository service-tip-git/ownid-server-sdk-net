using OwnIdSdk.NetCore3.Extensibility.Flow;

namespace OwnIdSdk.NetCore3.Flow.Steps
{
    /// <summary>
    ///     Stores information about step behavior
    /// </summary>
    public class FrontendBehavior
    {
        public FrontendBehavior()
        {
        }

        public FrontendBehavior(StepType type, ChallengeType challengeType, CallAction callback)
        {
            Type = type;
            ActionType = ActionType.Callback;
            Callback = callback;
            ChallengeType = challengeType;
        }

        public FrontendBehavior(StepType type, ChallengeType challengeType, PollingAction polling)
        {
            Type = type;
            ActionType = ActionType.Polling;
            Polling = polling;
            ChallengeType = challengeType;
        }

        /// <summary>
        ///     Step type
        /// </summary>
        public StepType Type { get; set; }

        /// <summary>
        ///     Action type that Web App should perform
        /// </summary>
        public ActionType ActionType { get; set; }

        /// <summary>
        ///     Challenge type to route shared <see cref="Type" />
        /// </summary>
        public ChallengeType ChallengeType { get; set; }

        /// <summary>
        ///     Polling action description
        /// </summary>
        /// <remarks>Should be null if not polling needed</remarks>
        public PollingAction Polling { get; set; }

        /// <summary>
        ///     Callback url for step data transfer
        /// </summary>
        public CallAction Callback { get; set; }
    }
}