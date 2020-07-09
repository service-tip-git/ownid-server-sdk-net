using System;
using System.Collections.Generic;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Store;

namespace OwnIdSdk.NetCore3.Flow
{
    public class FlowController
    {
        private readonly IOwnIdCoreConfiguration _coreConfiguration;

        private readonly IUrlProvider _urlProvider;

        public FlowController(IUrlProvider urlProvider, IOwnIdCoreConfiguration coreConfiguration)
        {
            _urlProvider = urlProvider;
            _coreConfiguration = coreConfiguration;
            InitMap();
        }

        public Step GetNextStep(CacheItem cacheItem, StepType currentStep)
        {
            return NextStepMap[cacheItem.FlowType][currentStep](cacheItem);
        }

        protected Dictionary<FlowType, Dictionary<StepType, Func<CacheItem, Step>>> NextStepMap { get; private set; }


        private void InitMap()
        {
            var authorize = new Dictionary<StepType, Func<CacheItem, Step>>
            {
                {
                    StepType.Starting,
                    cacheItem => new Step(StepType.Authorize, cacheItem.ChallengeType,
                        new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context, cacheItem.ChallengeType)))
                },
                {
                    StepType.Authorize, cacheItem => new Step
                    {
                        Type = StepType.Success,
                        ActionType = ActionType.Finish,
                        ChallengeType = cacheItem.ChallengeType
                    }
                }
            };

            var linkWithoutPin = new Dictionary<StepType, Func<CacheItem, Step>>
            {
                {
                    StepType.Starting,
                    cacheItem => new Step(StepType.Link, cacheItem.ChallengeType,
                        new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context, cacheItem.ChallengeType)))
                },
                {
                    StepType.Link, cacheItem => new Step
                    {
                        Type = StepType.Success,
                        ActionType = ActionType.Finish,
                        ChallengeType = cacheItem.ChallengeType
                    }
                }
            };

            var linkWithPin = new Dictionary<StepType, Func<CacheItem, Step>>
            {
                {
                    StepType.Starting,
                    cacheItem => new Step(StepType.ApprovePin, cacheItem.ChallengeType,
                        new PollingAction(_urlProvider.GetSecurityApprovalStatusUrl(cacheItem.Context),
                            _coreConfiguration.PollingInterval))
                },
                {
                    StepType.ApprovePin,
                    cacheItem =>
                    {
                        if (cacheItem.Status == CacheItemStatus.Approved)
                            return new Step(StepType.Link, cacheItem.ChallengeType,
                                new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context, cacheItem.ChallengeType)));
                        
                        return new Step
                        {
                            Type = StepType.Declined, 
                            ChallengeType = cacheItem.ChallengeType,
                            ActionType = ActionType.Finish
                        };
                    }
                },
                {
                    StepType.Link, cacheItem => new Step
                    {
                        Type = StepType.Success,
                        ChallengeType = cacheItem.ChallengeType,
                        ActionType = ActionType.Finish
                    }
                }
            };

            var recoverWithoutPin = new Dictionary<StepType, Func<CacheItem, Step>>
            {
                {
                    StepType.Starting,
                    cacheItem => new Step(StepType.Recover, cacheItem.ChallengeType,
                        new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context, cacheItem.ChallengeType)))
                },
                {
                    StepType.Recover, cacheItem => new Step
                    {
                        Type = StepType.Success,
                        ActionType = ActionType.Finish,
                        ChallengeType = cacheItem.ChallengeType
                    }
                }
            };

            var recoverWitPin = new Dictionary<StepType, Func<CacheItem, Step>>
            {
                {
                    StepType.Starting,
                    cacheItem => new Step(StepType.ApprovePin, cacheItem.ChallengeType,
                        new PollingAction(_urlProvider.GetSecurityApprovalStatusUrl(cacheItem.Context),
                            _coreConfiguration.PollingInterval))
                },
                {
                    StepType.ApprovePin,
                    cacheItem =>
                    {
                        if (cacheItem.Status == CacheItemStatus.Approved)
                            return new Step(StepType.Recover, cacheItem.ChallengeType,
                                new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context, cacheItem.ChallengeType)));
                        
                        return new Step
                        {
                            Type = StepType.Declined, 
                            ChallengeType = cacheItem.ChallengeType,
                            ActionType = ActionType.Finish
                        };
                    }
                },
                {
                    StepType.Recover, cacheItem => new Step
                    {
                        Type = StepType.Success,
                        ActionType = ActionType.Finish,
                        ChallengeType = cacheItem.ChallengeType
                    }
                }
            };

            NextStepMap = new Dictionary<FlowType, Dictionary<StepType, Func<CacheItem, Step>>>
            {
                {FlowType.Authorize, authorize},
                {FlowType.Link, linkWithoutPin},
                {FlowType.LinkWithPin, linkWithPin},
                {FlowType.Recover, recoverWithoutPin},
                {FlowType.RecoverWithPin, recoverWitPin}
            };
        }
    }
}