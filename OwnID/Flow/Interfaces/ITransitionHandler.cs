using System;
using System.Threading.Tasks;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Flow.ResultActions;

namespace OwnID.Flow.Interfaces
{
    public interface ITransitionHandler<TInput> : ITransitionHandler where TInput : ITransitionInput
    {
        public Func<TInput, CacheItem, FrontendBehavior> GetNextBehaviorFunc { get; set; }
    }

    public interface ITransitionHandler
    {
        public StepType StepType { get; }

        public Task<ITransitionResult> HandleAsync(ITransitionInput input, CacheItem relatedItem);
        
        public FrontendBehavior GetOwnReference(string context, ChallengeType challengeType);
    }
}