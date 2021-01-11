using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Start;
using OwnID.Flow.Interfaces;
using OwnID.Flow.ResultActions;
using OwnID.Flow.TransitionHandlers;

namespace OwnID.Flow.Setups
{
    public class BaseFlow
    {
        private readonly IServiceProvider _serviceProvider;

        public BaseFlow(IServiceProvider serviceProvider, FlowType flowType)
        {
            _serviceProvider = serviceProvider;
            Type = flowType;
        }

        public FlowType Type { get; }

        protected Dictionary<StepType, ITransitionHandler> TransitionHandlers { get; } = new();

        public async Task<ITransitionResult> RunAsync(ITransitionInput input, StepType currentStep, CacheItem cacheItem)
        {
            return await TransitionHandlers[currentStep].HandleAsync(input, cacheItem);
        }

        protected void AddStartingTransitions(StepType nextStep)
        {
            AddStartingTransitions((_, item) =>
                GetReferenceToExistingStep(nextStep, item.Context, item.ChallengeType));
        }

        protected virtual void AddStartingTransitions(
            Func<TransitionInput<AcceptStartRequest>, CacheItem, FrontendBehavior> nextStepDecider)
        {
            // 1. Starting
            AddHandler<StartFlowTransitionHandler, TransitionInput<StartRequest>>((input, item) =>
                GetReferenceToExistingStep(StepType.AcceptStart, input.Context, item.ChallengeType));

            // 2. AcceptStart
            AddHandler<AcceptStartTransitionHandler, TransitionInput<AcceptStartRequest>>(nextStepDecider);
        }

        protected void AddHandler<THandler, TTransitionInput>(
            Func<TTransitionInput, CacheItem, FrontendBehavior> nextStepDecider, Action<THandler> initAction = null)
            where THandler : ITransitionHandler<TTransitionInput> where TTransitionInput : ITransitionInput
        {
            var handler = _serviceProvider.GetService<THandler>();
            initAction?.Invoke(handler);
            handler.GetNextBehaviorFunc = nextStepDecider;
            TransitionHandlers.Add(handler.StepType, handler);
        }

        protected FrontendBehavior GetReferenceToExistingStep(StepType stepType, string context,
            ChallengeType challengeType)
        {
            return TransitionHandlers[stepType].GetOwnReference(context, challengeType);
        }
    }
}