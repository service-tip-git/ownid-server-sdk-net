using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OwnID.Commands;
using OwnID.Commands.Fido2;
using OwnID.Commands.Pin;
using OwnID.Commands.Recovery;
using OwnID.Configuration;
using OwnID.Cryptography;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Providers;
using OwnID.Flow;
using OwnID.Flow.Interfaces;
using OwnID.Flow.Setups.Fido2;
using OwnID.Flow.Setups.Partial;
using OwnID.Flow.TransitionHandlers;
using OwnID.Flow.TransitionHandlers.Fido2;
using OwnID.Flow.TransitionHandlers.Partial;
using OwnID.Providers;
using OwnID.Services;
using OwnID.Web.Extensibility;

namespace OwnID.Web.Features
{
    public class CoreFeature : IFeatureConfiguration
    {
        private readonly OwnIdCoreConfiguration _configuration;

        public CoreFeature()
        {
            _configuration = new OwnIdCoreConfiguration();
        }

        public void ApplyServices(IServiceCollection services)
        {
            services.TryAddSingleton(x => (IOwnIdCoreConfiguration) _configuration);

            services.TryAddSingleton<IJwtService, JwtService>();
            services.TryAddSingleton<IJwtComposer, JwtComposer>();
            services.TryAddSingleton<ICacheItemRepository, CacheItemRepository>();
            services.TryAddSingleton<IEncodingService, EncodingService>();
            services.TryAddSingleton<IUrlProvider, UrlProvider>();
            services.TryAddSingleton<IIdentitiesProvider, GuidIdentitiesProvider>();
            services.TryAddSingleton<ICookieService, CookieService>();

            // TODO: add interface to find all commands by it with reflection and inject
            services.TryAddSingleton<AddConnectionCommand>();
            services.TryAddSingleton<CheckUserExistenceCommand>();
            services.TryAddSingleton<CreateFlowCommand>();
            services.TryAddSingleton<GetStatusCommand>();
            services.TryAddSingleton<InternalConnectionRecoveryCommand>();
            services.TryAddSingleton<LinkAccountCommand>();
            services.TryAddSingleton<SavePartialConnectionCommand>();
            services.TryAddSingleton<SetNewEncryptionTokenCommand>();
            services.TryAddSingleton<StartFlowCommand>();
            services.TryAddSingleton<StopFlowCommand>();
            services.TryAddSingleton<TrySwitchToFido2FlowCommand>();
            services.TryAddSingleton<SetPinCommand>();
            services.TryAddSingleton<ApproveActionCommand>();
            services.TryAddSingleton<RecoverAccountCommand>();
            services.TryAddSingleton<SaveRecoveredAccountConnectionCommand>();
            services.TryAddSingleton<VerifyFido2CredentialIdCommand>();
            services.TryAddSingleton<GetFido2SettingsCommand>();

            services.TryAddTransient<AcceptStartTransitionHandler>();
            services.TryAddTransient<CheckUserExistenceBaseTransitionHandler>();
            services.TryAddTransient<UpgradeToFido2TransitionHandler>();
            services.TryAddTransient<UpgradeToPasscodeTransitionHandler>();
            services.TryAddTransient<PinApprovalStatusTransitionHandler>();
            services.TryAddTransient<StartFlowTransitionHandler>();
            services.TryAddTransient<StartFlowWithPinTransitionHandler>();

            services.TryAddTransient<ConnectionRestoreBaseTransitionHandler>();
            services.TryAddTransient<InstantAuthorizeBaseTransitionHandler>();
            services.TryAddTransient<LinkBaseTransitionHandler>();
            services.TryAddTransient<RecoverAcceptStartTransitionHandler>();
            services.TryAddTransient<RecoveryTransitionHandler>();
            services.TryAddTransient<StopFlowTransitionHandler>();

            services.TryAddSingleton<LinkFlow>();
            services.TryAddSingleton<LinkWithPinFlow>();
            services.TryAddSingleton<PartialAuthorizeFlow>();
            services.TryAddSingleton<RecoveryFlow>();
            services.TryAddSingleton<RecoveryWithPinFlow>();
            services.TryAddSingleton<IFlowRunner, FlowRunner>();

            services.TryAddSingleton<Fido2RegisterCommand>();
            services.TryAddSingleton<Fido2LoginCommand>();
            services.TryAddSingleton<Fido2LinkCommand>();
            services.TryAddSingleton<Fido2RecoveryCommand>();
            services.TryAddSingleton<Fido2UpgradeConnectionCommand>();

            services.TryAddTransient<Fido2LinkTransitionHandler>();
            services.TryAddTransient<Fido2LoginTransitionHandler>();
            services.TryAddTransient<Fido2RecoveryTransitionHandler>();
            services.TryAddTransient<Fido2RegisterTransitionHandler>();

            services.TryAddSingleton<Fido2LinkFlow>();
            services.TryAddSingleton<Fido2RecoveryFlow>();
            services.TryAddSingleton<Fido2LoginFlow>();
            services.TryAddSingleton<Fido2RegisterFlow>();

            services.AddFido2(fido2Config =>
            {
                if (_configuration.Fido2.IsEnabled)
                    fido2Config.Origin = _configuration.Fido2.Origin.ToString().TrimEnd('/').Trim();
            });
        }

        public IFeatureConfiguration FillEmptyWithOptional()
        {
            _configuration.OwnIdApplicationUrl ??= new Uri(Constants.OwinIdApplicationAddress);

            if (_configuration.CacheExpirationTimeout == default)
                _configuration.CacheExpirationTimeout = (uint) TimeSpan.FromMinutes(10).TotalMilliseconds;

            if (_configuration.JwtExpirationTimeout == default)
                _configuration.JwtExpirationTimeout = (uint) TimeSpan.FromMinutes(60).TotalMilliseconds;

            if (_configuration.PollingInterval == default)
                _configuration.PollingInterval = 2000;

            if (_configuration.MaximumNumberOfConnectedDevices == default)
                _configuration.MaximumNumberOfConnectedDevices = 1;

            if (string.IsNullOrWhiteSpace(_configuration.Fido2.RelyingPartyId))
                _configuration.Fido2.RelyingPartyId = _configuration.Fido2.PasswordlessPageUrl?.Host;

            if (string.IsNullOrWhiteSpace(_configuration.Fido2.RelyingPartyName))
                _configuration.Fido2.RelyingPartyName = _configuration.Name;

            if (string.IsNullOrWhiteSpace(_configuration.Fido2.UserName))
                _configuration.Fido2.UserName = "Skip the password";

            if (string.IsNullOrWhiteSpace(_configuration.Fido2.UserDisplayName))
                _configuration.Fido2.UserDisplayName = _configuration.Fido2.UserName;

            if (_configuration.Fido2.Origin == null)
                _configuration.Fido2.Origin = _configuration.Fido2.PasswordlessPageUrl;
            
            return this;
        }

        public void Validate()
        {
            // TODO refactor
            var validator = new OwnIdCoreConfigurationValidator();
            var result = validator.Validate(string.Empty, _configuration);

            if (result.Failed)
                throw new InvalidOperationException(result.FailureMessage);
        }

        public CoreFeature WithConfiguration(Action<IOwnIdCoreConfiguration> setupAction)
        {
            setupAction(_configuration);
            return this;
        }

        public CoreFeature WithKeys(string publicKeyPath, string privateKeyPath)
        {
            using var publicKeyReader = File.OpenText(publicKeyPath);
            using var privateKeyReader = File.OpenText(privateKeyPath);
            WithKeys(publicKeyReader, privateKeyReader);
            return this;
        }

        public CoreFeature WithKeys(TextReader publicKeyReader, TextReader privateKeyReader)
        {
            _configuration.JwtSignCredentials = RsaHelper.LoadKeys(publicKeyReader, privateKeyReader);
            return this;
        }

        public CoreFeature WithKeys(RSA rsa)
        {
            _configuration.JwtSignCredentials = rsa;
            return this;
        }
    }
}