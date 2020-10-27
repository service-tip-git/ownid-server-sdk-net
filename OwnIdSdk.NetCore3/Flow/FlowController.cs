using System.Collections.Generic;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Providers;
using OwnIdSdk.NetCore3.Flow.Commands;
using OwnIdSdk.NetCore3.Flow.Commands.Approval;
using OwnIdSdk.NetCore3.Flow.Commands.Authorize;
using OwnIdSdk.NetCore3.Flow.Commands.Fido2;
using OwnIdSdk.NetCore3.Flow.Commands.Internal;
using OwnIdSdk.NetCore3.Flow.Commands.Link;
using OwnIdSdk.NetCore3.Flow.Commands.Recovery;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;

namespace OwnIdSdk.NetCore3.Flow
{
    public class FlowController : IFlowController
    {
        private readonly IOwnIdCoreConfiguration _coreConfiguration;
        private readonly IUrlProvider _urlProvider;

        public FlowController(IUrlProvider urlProvider, IOwnIdCoreConfiguration coreConfiguration)
        {
            _urlProvider = urlProvider;
            _coreConfiguration = coreConfiguration;
            InitMap();
        }

        private Dictionary<FlowType, Dictionary<StepType, IStep>> StepMap { get; set; }

        public FrontendBehavior GetExpectedFrontendBehavior(CacheItem cacheItem, StepType currentStep)
        {
            return StepMap[cacheItem.FlowType][currentStep].GenerateFrontendBehavior(cacheItem);
        }

        public IStep GetStep(FlowType flowType, StepType currentStep)
        {
            return StepMap[flowType][currentStep];
        }

        private void InitMap()
        {
            var authorize = new Dictionary<StepType, IStep>
            {
                {
                    StepType.Starting,
                    new Step<StartFlowCommand>(cacheItem =>
                    {
                        var alternative = new FrontendBehavior(StepType.Authorize, cacheItem.ChallengeType,
                            new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context, cacheItem.ChallengeType)));

                        var mainBehavior = new FrontendBehavior(StepType.InternalConnectionRecovery,
                            cacheItem.ChallengeType,
                            new CallAction(_urlProvider.GetConnectionRecoveryUrl(cacheItem.Context)),
                            alternative);

                        return mainBehavior;
                    })
                },
                {
                    StepType.InternalConnectionRecovery,
                    new Step<InternalConnectionRecoveryCommand>(cacheItem =>
                        new FrontendBehavior(StepType.Authorize, cacheItem.ChallengeType,
                            new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context, cacheItem.ChallengeType))))
                },
                {
                    StepType.Authorize, new Step<SaveProfileCommand>(cacheItem => new FrontendBehavior
                    {
                        Type = StepType.Success,
                        ActionType = ActionType.Finish,
                        ChallengeType = cacheItem.ChallengeType
                    })
                }
            };

            var partialAuthorize = new Dictionary<StepType, IStep>
            {
                {
                    StepType.Starting,
                    new Step<StartFlowCommand>(cacheItem =>
                    {
                        var alternative = new FrontendBehavior(StepType.InstantAuthorize, cacheItem.ChallengeType,
                            new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context, cacheItem.ChallengeType,
                                "/partial")));

                        var mainBehavior = new FrontendBehavior(StepType.InternalConnectionRecovery,
                            cacheItem.ChallengeType,
                            new CallAction(_urlProvider.GetConnectionRecoveryUrl(cacheItem.Context)),
                            alternative);

                        return mainBehavior;
                    })
                },
                {
                    StepType.InternalConnectionRecovery,
                    new Step<InternalConnectionRecoveryCommand>(cacheItem => new FrontendBehavior(
                        StepType.InstantAuthorize, cacheItem.ChallengeType,
                        new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context, cacheItem.ChallengeType,
                            "/partial"))))
                },
                {
                    StepType.InstantAuthorize,
                    new Step<SavePartialProfileCommand>(cacheItem => new FrontendBehavior
                    {
                        Type = StepType.Success,
                        ActionType = ActionType.Finish,
                        ChallengeType = cacheItem.ChallengeType
                    })
                }
            };

            var link = new Dictionary<StepType, IStep>
            {
                {
                    StepType.Starting, new Step<StartFlowCommand>(cacheItem => new FrontendBehavior(StepType.Link,
                        cacheItem.ChallengeType,
                        new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context, cacheItem.ChallengeType))))
                },
                {
                    StepType.Link, new Step<SaveAccountLinkCommand>(cacheItem => new FrontendBehavior
                    {
                        Type = string.IsNullOrEmpty(cacheItem.Error) ? StepType.Success : StepType.Error,
                        ActionType = ActionType.Finish,
                        ChallengeType = cacheItem.ChallengeType
                    })
                }
            };

            var linkWithPin = new Dictionary<StepType, IStep>
            {
                {
                    StepType.Starting,
                    new Step<StartFlowCommand>(cacheItem => new FrontendBehavior(StepType.ApprovePin,
                        cacheItem.ChallengeType,
                        new PollingAction(_urlProvider.GetSecurityApprovalStatusUrl(cacheItem.Context),
                            _coreConfiguration.PollingInterval)))
                },
                {
                    StepType.ApprovePin,
                    new Step<GetApprovalStatusCommand>(cacheItem =>
                    {
                        if (cacheItem.Status == CacheItemStatus.Approved)
                            return new FrontendBehavior(StepType.Link, cacheItem.ChallengeType,
                                new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context,
                                    cacheItem.ChallengeType)));

                        return new FrontendBehavior
                        {
                            Type = StepType.Declined,
                            ChallengeType = cacheItem.ChallengeType,
                            ActionType = ActionType.Finish
                        };
                    })
                },
                {
                    StepType.Link,
                    new Step<SaveAccountLinkCommand>(cacheItem => new FrontendBehavior
                    {
                        Type = string.IsNullOrEmpty(cacheItem.Error) ? StepType.Success : StepType.Error,
                        ChallengeType = cacheItem.ChallengeType,
                        ActionType = ActionType.Finish
                    })
                }
            };

            var recover = new Dictionary<StepType, IStep>
            {
                {
                    StepType.Starting,
                    new Step<StartFlowCommand>(cacheItem => new FrontendBehavior(StepType.Recover,
                        cacheItem.ChallengeType,
                        new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context, cacheItem.ChallengeType))))
                },
                {
                    StepType.Recover, new Step<SaveAccountPublicKeyCommand>(cacheItem => new FrontendBehavior
                    {
                        Type = string.IsNullOrEmpty(cacheItem.Error) ? StepType.Success : StepType.Error,
                        ActionType = ActionType.Finish,
                        ChallengeType = cacheItem.ChallengeType
                    })
                }
            };

            var recoverWitPin = new Dictionary<StepType, IStep>
            {
                {
                    StepType.Starting,
                    new Step<StartFlowCommand>(cacheItem => new FrontendBehavior(StepType.ApprovePin,
                        cacheItem.ChallengeType,
                        new PollingAction(_urlProvider.GetSecurityApprovalStatusUrl(cacheItem.Context),
                            _coreConfiguration.PollingInterval)))
                },
                {
                    StepType.ApprovePin,
                    new Step<GetApprovalStatusCommand>(cacheItem =>
                    {
                        if (cacheItem.Status == CacheItemStatus.Approved)
                            return new FrontendBehavior(StepType.Recover, cacheItem.ChallengeType,
                                new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context,
                                    cacheItem.ChallengeType)));

                        return new FrontendBehavior
                        {
                            Type = StepType.Declined,
                            ChallengeType = cacheItem.ChallengeType,
                            ActionType = ActionType.Finish
                        };
                    })
                },
                {
                    StepType.Recover, new Step<SaveAccountPublicKeyCommand>(cacheItem => new FrontendBehavior
                    {
                        Type = string.IsNullOrEmpty(cacheItem.Error) ? StepType.Success : StepType.Error,
                        ActionType = ActionType.Finish,
                        ChallengeType = cacheItem.ChallengeType
                    })
                }
            };

            var fido2Register = new Dictionary<StepType, IStep>
            {
                {
                    StepType.Starting,
                    new Step<StartFlowCommand>(cacheItem => new FrontendBehavior(StepType.Fido2Authorize,
                        cacheItem.ChallengeType,
                        new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context, cacheItem.ChallengeType,
                            "/fido2"))))
                },
                {
                    StepType.Fido2Authorize,
                    new Step<Fido2RegisterCommand>(cacheItem => new FrontendBehavior
                    {
                        Type = StepType.Success,
                        ActionType = ActionType.Finish,
                        ChallengeType = cacheItem.ChallengeType
                    })
                }
            };

            var fido2Login = new Dictionary<StepType, IStep>
            {
                {
                    StepType.Starting,
                    new Step<StartFlowCommand>(cacheItem => new FrontendBehavior(StepType.Fido2Authorize, cacheItem.ChallengeType,
                        new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context, cacheItem.ChallengeType,
                            "/fido2"))))
                },
                {
                    StepType.Fido2Authorize,
                    new Step<Fido2LoginCommand>(cacheItem => new FrontendBehavior
                    {
                        Type = StepType.Success,
                        ActionType = ActionType.Finish,
                        ChallengeType = cacheItem.ChallengeType
                    })
                }
            };

            var fido2Link = new Dictionary<StepType, IStep>
            {
                {
                    StepType.Starting,
                    new Step<StartFlowCommand>(cacheItem => new FrontendBehavior(StepType.Fido2Authorize,
                        cacheItem.ChallengeType,
                        new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context, cacheItem.ChallengeType,
                            "/fido2"))))
                },
                {
                    StepType.Fido2Authorize,
                    new Step<Fido2LinkCommand>(cacheItem => new FrontendBehavior
                    {
                        Type = StepType.Success,
                        ActionType = ActionType.Finish,
                        ChallengeType = cacheItem.ChallengeType
                    })
                }
            };

            var fido2Recover = new Dictionary<StepType, IStep>
            {
                {
                    StepType.Starting,
                    new Step<StartFlowCommand>(cacheItem => new FrontendBehavior(StepType.Fido2Authorize,
                        cacheItem.ChallengeType,
                        new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context, cacheItem.ChallengeType,
                            "/fido2"))))
                },
                {
                    StepType.Fido2Authorize,
                    new Step<Fido2RecoverCommand>(cacheItem => new FrontendBehavior
                    {
                        Type = StepType.Success,
                        ActionType = ActionType.Finish,
                        ChallengeType = cacheItem.ChallengeType
                    })
                }
            };
            
            StepMap = new Dictionary<FlowType, Dictionary<StepType, IStep>>
            {
                {FlowType.Authorize, authorize},
                {FlowType.PartialAuthorize, partialAuthorize},
                {FlowType.Link, link},
                {FlowType.LinkWithPin, linkWithPin},
                {FlowType.Recover, recover},
                {FlowType.RecoverWithPin, recoverWitPin},
                {FlowType.Fido2Register, fido2Register},
                {FlowType.Fido2Login, fido2Login},
                {FlowType.Fido2Link, fido2Link},
                {FlowType.Fido2LinkWithPin, GetFido2LinkAndRecoveryWithPinSteps<Fido2LinkWithPinCommand>()},
                {FlowType.Fido2Recover, fido2Recover},
                {FlowType.Fido2RecoverWithPin, GetFido2LinkAndRecoveryWithPinSteps<Fido2RecoverWithPinCommand>()}
            };
        }

        private Dictionary<StepType, IStep> GetFido2LinkAndRecoveryWithPinSteps<TFido2FinishCommand>()
            where TFido2FinishCommand: BaseFido2RegisterCommand
        {
            return new Dictionary<StepType, IStep>
            {
                {
                    StepType.Starting,
                    new Step<StartFlowCommand>(cacheItem => new FrontendBehavior(StepType.ApprovePin,
                        cacheItem.ChallengeType,
                        new PollingAction(_urlProvider.GetSecurityApprovalStatusUrl(cacheItem.Context),
                            _coreConfiguration.PollingInterval)))
                },
                {
                    StepType.ApprovePin,
                    new Step<GetApprovalStatusCommand>(cacheItem =>
                    {
                        if (cacheItem.Status == CacheItemStatus.Approved)
                            return new FrontendBehavior(StepType.Fido2Authorize, cacheItem.ChallengeType,
                                new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context,
                                    cacheItem.ChallengeType, "/fido2")));

                        return new FrontendBehavior
                        {
                            Type = StepType.Declined,
                            ChallengeType = cacheItem.ChallengeType,
                            ActionType = ActionType.Finish
                        };
                    })
                },
                {
                    StepType.Fido2Authorize,
                    new Step<TFido2FinishCommand>(cacheItem => new FrontendBehavior
                    {
                        Type = StepType.Success,
                        ChallengeType = cacheItem.ChallengeType,
                        ActionType = ActionType.Finish
                    })
                }
            };
        }
    }
}