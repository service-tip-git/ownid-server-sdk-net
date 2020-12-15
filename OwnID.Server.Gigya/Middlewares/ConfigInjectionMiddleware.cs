using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Json;
using OwnID.Flow.Interfaces;

namespace OwnID.Server.Gigya.Middlewares
{
    public class ConfigInjectionMiddleware
    {
        private readonly IOwnIdCoreConfiguration _configuration;
        private readonly IFlowController _flowController;

        public ConfigInjectionMiddleware(RequestDelegate next, IOwnIdCoreConfiguration configuration,
            IFlowController flowController)
        {
            _configuration = configuration;
            _flowController = flowController;
        }

        public async Task Invoke(HttpContext context)
        {
            var config = await OwnIdSerializer.DeserializeAsync<ConfigToInject>(context.Request.Body);

            _configuration.TFAEnabled = config.TFAEnabled;
            _configuration.Fido2FallbackBehavior = config.Fido2FallbackBehavior;

            var initFlowControllerMethod = _flowController.GetType()
                .GetMethod("InitMap", BindingFlags.NonPublic | BindingFlags.Instance);
            initFlowControllerMethod!.Invoke(_flowController, null);

            context.Response.StatusCode = StatusCodes.Status204NoContent;
        }

        private class ConfigToInject
        {
            public bool TFAEnabled { get; set; }

            public Fido2FallbackBehavior Fido2FallbackBehavior { get; set; }
        }
    }
}