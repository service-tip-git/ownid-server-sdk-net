using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Json;
using OwnIdSdk.NetCore3.Extensibility.Services;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class MetricsMiddleware
    {
        private class ContextStatus
        {
            public CacheItemStatus Status { get; set; }
            public string Context { get; set; }

            public StatusPayload Payload { get; set; }

            public class StatusPayload
            {
                public string Error { get; set; }
            }
        }

        private readonly RequestDelegate _next;
        private readonly ICacheItemService _cacheItemService;
        private readonly IMetricsService _metricsService;
        private readonly ILogger<MetricsMiddleware> _logger;

        private static readonly Regex ContextRegex = new Regex(@"ownid\/(?<context>[^\/]*)\/.*", RegexOptions.Compiled);

        public MetricsMiddleware(RequestDelegate next, ICacheItemService cacheItemService,
            IMetricsService metricsService, ILogger<MetricsMiddleware> logger)
        {
            _next = next;
            _cacheItemService = cacheItemService;
            _metricsService = metricsService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            // Process request
            httpContext.Request.EnableBuffering();
            try
            {
                await ProcessRequestAsync(httpContext);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
            }

            httpContext.Request.Body.Position = 0;

            var originalBodyStream = httpContext.Response.Body;
            await using var responseBody = new MemoryStream();
            httpContext.Response.Body = responseBody;

            await _next(httpContext);

            // Process response
            try
            {
                await ProcessResponseAsync(httpContext);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
            }

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }

        private string GetOwnIdContext(HttpContext httpContext)
        {
            var match = ContextRegex.Match(httpContext.Request.Path.Value);
            return match.Success ? match.Groups["context"].Value : null;
        }
        
        private async Task ProcessRequestAsync(HttpContext httpContext)
        {
            if (!httpContext.Request.Path.HasValue
                || httpContext.Request.Method != HttpMethod.Post.Method
                || !httpContext.Request.Path.Value.EndsWith("/start/state")
            )
            {
                return;
            }

            // Do not log if this is second call of "start/state" endpoint
            //
            // TODO: consider rewriting in the way to not use the same endpoint for different actions
            //
            httpContext.Request.EnableBuffering();
            var request = await (new StreamReader(httpContext.Request.Body)).ReadToEndAsync();
            if (request.Contains("\"credId\":\""))
            {
                httpContext.Request.Body.Position = 0;
                return;
            }
            httpContext.Request.Body.Position = 0;

            var context = GetOwnIdContext(httpContext);
            if (string.IsNullOrEmpty(context))
                return;

            var item = await _cacheItemService.GetCacheItemByContextAsync(context);
            if (item == null)
                return;

            await _metricsService.LogAsync(item.ChallengeType.ToString());
        }

        private async Task ProcessResponseAsync(HttpContext httpContext)
        {
            if (!httpContext.Request.Path.HasValue
                || httpContext.Request.Method != HttpMethod.Post.Method
                || !httpContext.Request.Path.Value.EndsWith("/status"))
            {
                return;
            }

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var response = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();

            var statuses = OwnIdSerializer.Deserialize<List<ContextStatus>>(response);
            foreach (var item in statuses.Where(i => i.Status == CacheItemStatus.Finished))
            {
                var cacheItem = await _cacheItemService.GetCacheItemByContextAsync(item.Context);
                if (cacheItem == null)
                    continue;

                var result = string.IsNullOrEmpty(item.Payload?.Error) ? "success" : "error";
                await _metricsService.LogAsync($"{cacheItem.ChallengeType} {result}");
            }
        }
    }
}