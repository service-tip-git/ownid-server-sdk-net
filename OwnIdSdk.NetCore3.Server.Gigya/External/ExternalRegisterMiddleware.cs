using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Web.Gigya;
using OwnIdSdk.NetCore3.Web.Gigya.ApiClient;
using OwnIdSdk.NetCore3.Web.Gigya.Contracts;

namespace OwnIdSdk.NetCore3.Server.Gigya.External
{
    public class ExternalRegisterMiddleware
    {
        private readonly GigyaRestApiClient<GigyaUserProfile> _gigyaRest;
        private readonly ILogger<LogRequestMiddleware> _logger;
        private readonly RequestDelegate _next;

        public ExternalRegisterMiddleware(RequestDelegate next, ILogger<LogRequestMiddleware> logger,
            GigyaConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _next = next;
            _logger = logger;
            _gigyaRest = new GigyaRestApiClient<GigyaUserProfile>(configuration, httpClientFactory);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var barRequestResponse = new Func<string, Task>(async (message) =>
            {
                context.Response.ContentType = "application/json";
                await context.Response.Body.WriteAsync(
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
                    {
                        error = message
                    })));
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            });

            var profile = await JsonSerializer.DeserializeAsync<ProfileData>(context.Request.Body,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            
            if (string.IsNullOrEmpty(profile.Email) || string.IsNullOrEmpty(profile.PubKey))
            {
                await barRequestResponse($"{nameof(profile.Email)} and {nameof(profile.PubKey)} are required");
                return;
            }

            var newUserId = $"partial:{Guid.NewGuid()}";
            var loginResponse = await _gigyaRest.NotifyLogin(newUserId, "browser");

            if (loginResponse.ErrorCode != 0)
            {
                await barRequestResponse(loginResponse.GetFailureMessage());
                return;
            }

            var setAccountMessage =
                await _gigyaRest.SetAccountInfo(newUserId, new GigyaUserProfile
                    {
                        Email = profile.Email,
                        FirstName = profile.FirstName
                    },
                    new AccountData(profile.PubKey));

            if (setAccountMessage.ErrorCode == 403043)
            {
                var removeUserResult = await _gigyaRest.DeleteAccountAsync(newUserId);

                if (removeUserResult.ErrorCode != 0)
                    _logger.LogError($"[NOT OWNID] Gigya.deleteAccount with uid={newUserId} error -> {removeUserResult.GetFailureMessage()}");
                
                return;
            }

            if (setAccountMessage.ErrorCode > 0)
            {
                await barRequestResponse(setAccountMessage.GetFailureMessage());
                return;
            }

            var loginMessage = await _gigyaRest.NotifyLogin(newUserId, "browser");
            
            if (loginMessage.ErrorCode > 0)
            {
                await barRequestResponse(loginMessage.GetFailureMessage());
                return;
            }
            
            context.Response.ContentType = "application/json";
            await context.Response.Body.WriteAsync(
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new {
                sessionInfo = loginMessage.SessionInfo,
                identities = loginMessage.Identities.FirstOrDefault()
            })));
            context.Response.StatusCode = StatusCodes.Status200OK;
        }

        private class ProfileData
        {
            public string Email { get; set; }

            public string FirstName { get; set; }

            public string PubKey { get; set; }
        }
    }
}