using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Cryptography;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Providers;
using OwnIdSdk.NetCore3.Flow;
using OwnIdSdk.NetCore3.Flow.Commands;
using OwnIdSdk.NetCore3.Flow.Commands.Approval;
using OwnIdSdk.NetCore3.Flow.Commands.Authorize;
using OwnIdSdk.NetCore3.Flow.Commands.Fido2;
using OwnIdSdk.NetCore3.Flow.Commands.Internal;
using OwnIdSdk.NetCore3.Flow.Commands.Link;
using OwnIdSdk.NetCore3.Flow.Commands.Recovery;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Providers;
using OwnIdSdk.NetCore3.Services;
using OwnIdSdk.NetCore3.Web.Extensibility;

namespace OwnIdSdk.NetCore3.Web.Features
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
            services.TryAddSingleton<ICacheItemService, CacheItemService>();
            services.TryAddSingleton<IUrlProvider, UrlProvider>();
            services.TryAddSingleton<IIdentitiesProvider, GuidIdentitiesProvider>();

            // TODO: add interface to find all commands by it with reflection and inject
            services.TryAddSingleton<CreateFlowCommand>();
            services.TryAddSingleton<GetSecurityCheckCommand>();
            services.TryAddSingleton<GetStatusCommand>();
            services.TryAddSingleton<StartFlowCommand>();
            services.TryAddSingleton<ApproveActionCommand>();
            services.TryAddSingleton<GetApprovalStatusCommand>();
            services.TryAddSingleton<GetAuthProfileCommand>();
            services.TryAddSingleton<GetPartialInfoCommand>();
            services.TryAddSingleton<SavePartialProfileCommand>();
            services.TryAddSingleton<SaveProfileCommand>();
            services.TryAddSingleton<GetNextStepCommand>();
            services.TryAddSingleton<SaveAccountLinkCommand>();
            services.TryAddSingleton<RecoverAccountCommand>();
            services.TryAddSingleton<SaveAccountPublicKeyCommand>();
            services.TryAddSingleton<InternalConnectionRecoveryCommand>();
            services.TryAddSingleton<SetPasswordlessStateCommand>();
            services.TryAddSingleton<SetWebAppStateCommand>();

            services.TryAddSingleton<IFlowController, FlowController>();
            services.TryAddSingleton<IFlowRunner, FlowRunner>();
            
            services.TryAddSingleton<IsFido2UserExistsCommand>();

            if (_configuration.AuthenticationMode.IsFido2Enabled())
            {
                services.TryAddSingleton<Fido2RegisterCommand>();
                services.TryAddSingleton<Fido2LoginCommand>();

                services.TryAddSingleton<Fido2LinkCommand>();
                services.TryAddSingleton<Fido2GetSecurityCheckCommand>();
                services.TryAddSingleton<Fido2LinkWithPinCommand>();

                services.TryAddSingleton<Fido2RecoverCommand>();
                services.TryAddSingleton<Fido2RecoverWithPinCommand>();
                services.TryAddSingleton<Fido2RecoverWithPinCommand>();
                
                services.AddFido2(fido2Config =>
                {
                    var str = _configuration.Fido2.Origin.ToString().TrimEnd(new[] {'/'}).Trim();
                    fido2Config.Origin = str;
                });
            }
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