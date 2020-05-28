using Microsoft.Extensions.DependencyInjection;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Gigya
{
    public static class OwnIdConfigurationBuilderExtension
    {
        public static void UseGigya(this IExtendableConfigurationBuilder builder, string segment, string apiKey,
            string secret, string authSecret)
        {
            builder.Services.AddHttpClient();
            UserHandler.ApiKey = apiKey;
            UserHandler.SecretKey = secret;
            UserHandler.AuthSecret = authSecret;
            UserHandler.Segment = segment;
            builder.UseUserHandlerWithCustomProfile<Profile, UserHandler>();
        }
    }
}