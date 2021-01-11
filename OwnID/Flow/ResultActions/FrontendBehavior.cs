using System;
using System.Net.Http;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow;

namespace OwnID.Flow.ResultActions
{
    /// <summary>
    ///     Stores information about step behavior
    /// </summary>
    public class FrontendBehavior
    {
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

        public FrontendBehavior(StepType type, ChallengeType challengeType, FrontendBehavior nextBehavior)
        {
            Type = type;
            ActionType = ActionType.GoToNext;
            ChallengeType = challengeType;
            NextBehavior = nextBehavior;
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
        ///     Next expected behavior
        /// </summary>
        public FrontendBehavior NextBehavior { get; set; }

        /// <summary>
        ///     Alternative behavior
        /// </summary>
        public FrontendBehavior AlternativeBehavior { get; set; }

        /// <summary>
        ///     Error code
        /// </summary>
        /// <remarks>
        ///     Has value only if <see cref="Type" /> is <see cref="StepType.Error" />
        /// </remarks>
        public ErrorType? Error { get; set; }

        public static FrontendBehavior CreateError(ErrorType errorType)
        {
            return new()
            {
                ActionType = ActionType.Finish,
                Type = StepType.Error,
                Error = errorType
            };
        }

        public static FrontendBehavior CreateSuccessFinish(ChallengeType challengeType)
        {
            return new()
            {
                Type = StepType.Success,
                ChallengeType = challengeType,
                ActionType = ActionType.Finish
            };
        }

        public static FrontendBehavior CreateRedirect(Uri uri)
        {
            return new()
            {
                ActionType = ActionType.Redirect,
                Callback = new CallAction(uri, HttpMethod.Get.Method),
                Type = StepType.Success
            };
        }
    }
}