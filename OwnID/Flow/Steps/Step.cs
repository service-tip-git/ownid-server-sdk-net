using System;
using OwnID.Flow.Commands;
using OwnID.Extensibility.Cache;

namespace OwnID.Flow.Steps
{
    public interface IStep
    {
        FrontendBehavior GenerateFrontendBehavior(CacheItem item);
        Type GetRelatedCommandType();
    }

    public class Step<T> : IStep where T : BaseFlowCommand
    {
        private readonly Func<CacheItem, FrontendBehavior> _frontBehaviorGenerator;

        public Step(Func<CacheItem, FrontendBehavior> frontBehaviorGenerator)
        {
            _frontBehaviorGenerator = frontBehaviorGenerator;
        }

        public FrontendBehavior GenerateFrontendBehavior(CacheItem item)
        {
            return _frontBehaviorGenerator(item);
        }

        public Type GetRelatedCommandType()
        {
            return typeof(T);
        }
    }
}