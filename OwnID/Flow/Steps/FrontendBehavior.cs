using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow;

namespace OwnID.Flow.Steps
{
    /// <summary>
    ///     Stores information about step behavior
    /// </summary>
    public class FrontendBehavior
    {
        public static FrontendBehavior CreateError(ErrorType errorType)
        {
            return new FrontendBehavior
            {
                ActionType = ActionType.Finish,
                Type = StepType.Error,
                Error = errorType
            };
        }
        
        public FrontendBehavior()
        {
        }

        public FrontendBehavior(StepType type, ChallengeType challengeType, CallAction callback,
            FrontendBehavior alternativeBehavior = null)
        {
            Type = type;
            ActionType = ActionType.Callback;
            Callback = callback;
            ChallengeType = challengeType;
            AlternativeBehavior = alternativeBehavior;
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

        /// <summary>
        ///     Alternative behavior
        /// </summary>
        public FrontendBehavior AlternativeBehavior { get; set; }
        
        /// <summary>
        ///     Error code
        /// </summary>
        /// <remarks>
        ///    Has value only if <see cref="Type"/> is <see cref="StepType.Error"/>
        /// </remarks>
        public ErrorType? Error { get; set; }
    }
}