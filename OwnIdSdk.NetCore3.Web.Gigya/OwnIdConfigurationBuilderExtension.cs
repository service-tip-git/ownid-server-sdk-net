using Microsoft.Extensions.DependencyInjection;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Gigya
{
    public static class OwnIdConfigurationBuilderExtension
    {
        /// <summary>
        ///     Enables GIGYA authorization process with <see cref="GigyaUserHandler" /> and <see cref="GigyaUserProfile" />
        /// </summary>
        /// <param name="dataCenter">
        ///     Gigya data center servers (us1, eu1 and etc.) If you are not sure of your site's data center,
        ///     <see cref="https://developers.gigya.com/display/GD/Finding+Your+Data+Center">Finding Your Data Center</see>.
        /// </param>
        /// <param name="apiKey">Gigya API key</param>
        /// <param name="secret">Gigya Secret key</param>
        /// <param name="loginType">Login result. Cookie session or JWT ID Token</param>
        public static void UseGigya(this IExtendableConfigurationBuilder builder, string dataCenter, string apiKey,
            string secret, GigyaLoginType loginType = GigyaLoginType.Session)
        {
            builder.Services.AddHttpClient();
            var gigyaFeature = new GigyaIntegrationFeature();
            
            gigyaFeature.WithConfig(x =>
            {
                x.DataCenter = dataCenter;
                x.ApiKey = apiKey;
                x.SecretKey = secret;
                x.LoginType = loginType;
            });
            
            builder.AddOrUpdateFeature(gigyaFeature);
            builder.UseUserHandlerWithCustomProfile<GigyaUserProfile, GigyaUserHandler>();
            builder.UseAccountLinking<GigyaUserProfile, GigyaAccountLinkHandler>();
            builder.UseAccountRecovery<GigyaAccountRecoveryHandler>();
        }
    }
}