using System;
using System.Collections.Generic;
using System.Net.Http;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Providers;
using OwnID.Flow.Commands;
using OwnID.Flow.Commands.Approval;
using OwnID.Flow.Commands.Authorize;
using OwnID.Flow.Commands.Fido2;
using OwnID.Flow.Commands.Internal;
using OwnID.Flow.Commands.Link;
using OwnID.Flow.Commands.Recovery;
using OwnID.Flow.Interfaces;
using OwnID.Flow.Steps;

namespace OwnID.Flow
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
                    GetFallbackStartingStep(cacheItem =>
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
                    StepType.Authorize, GetFinishStep<SaveProfileCommand>()
                }
            };

            var partialAuthorize = new Dictionary<StepType, IStep>
            {
                {
                    StepType.Starting,
                    GetFallbackStartingStep(cacheItem =>
                    {
                        var authorizeStep = new FrontendBehavior(
                            StepType.InstantAuthorize, cacheItem.ChallengeType,
                            new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context, cacheItem.ChallengeType,
                                "/partial")));
                        var internalRecovery = new FrontendBehavior(StepType.InternalConnectionRecovery,
                            cacheItem.ChallengeType,
                            new CallAction(_urlProvider.GetConnectionRecoveryUrl(cacheItem.Context)),
                            authorizeStep);
                        if (cacheItem.ChallengeType != ChallengeType.Register)
                            return internalRecovery;
                        var checkUser = new FrontendBehavior(StepType.CheckUserExistence, cacheItem.ChallengeType,
                            new CallAction(_urlProvider.GetUserExistenceUrl(cacheItem.Context)), authorizeStep);
                        if (!string.IsNullOrEmpty(cacheItem.RecoveryToken))
                            checkUser.AlternativeBehavior = internalRecovery;
                        return checkUser;
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
                    StepType.CheckUserExistence,
                    new Step<CheckUserExistenceCommand>(cacheItem => new FrontendBehavior(
                        StepType.InstantAuthorize, cacheItem.ChallengeType,
                        new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context, cacheItem.ChallengeType,
                            "/partial"))))
                },
                {
                    StepType.InstantAuthorize,
                    GetFinishStep<SavePartialProfileCommand>()
                }
            };

            var link = new Dictionary<StepType, IStep>
            {
                {
                    StepType.Starting,
                    GetFallbackStartingStep(cacheItem => new FrontendBehavior(StepType.Link,
                        cacheItem.ChallengeType,
                        new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context, cacheItem.ChallengeType))))
                },
                {
                    StepType.Link, GetFinishStep<SaveAccountLinkCommand>()
                }
            };

            var linkWithPin = new Dictionary<StepType, IStep>
            {
                {
                    StepType.Starting,
                    GetFallbackStartingStep(cacheItem => new FrontendBehavior(StepType.ApprovePin,
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
                    GetFinishStep<SaveAccountLinkCommand>()
                }
            };

            var recover = new Dictionary<StepType, IStep>
            {
                {
                    StepType.Starting,
                    GetFallbackStartingStep(cacheItem => new FrontendBehavior(StepType.Recover, cacheItem.ChallengeType,
                        new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context, cacheItem.ChallengeType))))
                },
                {
                    StepType.Recover, GetFinishStep<SaveAccountPublicKeyCommand>()
                }
            };

            var recoverWitPin = new Dictionary<StepType, IStep>
            {
                {
                    StepType.Starting,
                    GetFallbackStartingStep(cacheItem => new FrontendBehavior(StepType.ApprovePin,
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
                    StepType.Recover, GetFinishStep<SaveAccountPublicKeyCommand>()
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
                    StepType.Fido2Authorize, GetFinishStep<Fido2RegisterCommand>()
                }
            };

            var fido2Login = new Dictionary<StepType, IStep>
            {
                {
                    StepType.Starting,
                    new Step<StartFlowCommand>(cacheItem => new FrontendBehavior(StepType.Fido2Authorize,
                        cacheItem.ChallengeType,
                        new CallAction(_urlProvider.GetChallengeUrl(cacheItem.Context, cacheItem.ChallengeType,
                            "/fido2"))))
                },
                {
                    StepType.Fido2Authorize, GetFinishStep<Fido2LoginCommand>()
                }
            };

            var fido2Link = new Dictionary<StepType, IStep>
            {
                {
                    StepType.Starting,
                    new Step<StartFlowCommand>(cacheItem => new FrontendBehavior(StepType.Fido2Authorize,
                        cacheItem.ChallengeType,
                        new CallAction(
                            _urlProvider.GetChallengeUrl(cacheItem.Context, ChallengeType.Register, "/fido2"))))
                },
                {
                    StepType.Fido2Authorize,
                    GetFinishStep<Fido2LinkCommand>()
                }
            };

            var fido2Recover = new Dictionary<StepType, IStep>
            {
                {
                    StepType.Starting,
                    new Step<StartFlowCommand>(cacheItem => new FrontendBehavior(StepType.Fido2Authorize,
                        cacheItem.ChallengeType,
                        new CallAction(
                            _urlProvider.GetChallengeUrl(cacheItem.Context, ChallengeType.Register, "/fido2"))))
                },
                {
                    StepType.Fido2Authorize,
                    GetFinishStep<Fido2RecoverCommand>()
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
            where TFido2FinishCommand : BaseFido2RegisterCommand
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
                                    ChallengeType.Register, "/fido2")));

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
                    GetFinishStep<TFido2FinishCommand>()
                }
            };
        }

        private Step<TCommand> GetFinishStep<TCommand>() where TCommand : BaseFlowCommand
        {
            return new Step<TCommand>(cacheItem => new FrontendBehavior
            {
                Type = string.IsNullOrEmpty(cacheItem.Error) ? StepType.Success : StepType.Error,
                ChallengeType = cacheItem.ChallengeType,
                ActionType = ActionType.Finish
            });
        }

        private Step<StartFlowCommand> GetFallbackStartingStep(Func<CacheItem, FrontendBehavior> frontBehaviorGenerator)
        {
            if (!_coreConfiguration.TFAEnabled
                || _coreConfiguration.Fido2FallbackBehavior != Fido2FallbackBehavior.Passcode)
                return new Step<StartFlowCommand>(frontBehaviorGenerator);

            return new Step<StartFlowCommand>(cacheItem =>
                new FrontendBehavior(StepType.EnterPasscode, cacheItem.ChallengeType, frontBehaviorGenerator(cacheItem))
                {
                    AlternativeBehavior = new FrontendBehavior(StepType.ResetPasscode, cacheItem.ChallengeType,
                        new CallAction(_urlProvider.GetResetPasscodeUrl(cacheItem.Context),
                            HttpMethod.Delete.ToString()))
                });
        }
    }
}