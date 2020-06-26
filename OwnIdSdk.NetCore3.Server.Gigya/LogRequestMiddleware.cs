using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Web;

namespace OwnIdSdk.NetCore3.Server.Gigya
{
    public class LogRequestMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LogRequestMiddleware> _logger;

        public LogRequestMiddleware(RequestDelegate next, ILogger<LogRequestMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using (_logger.BeginScope("Log request ({requestId})", context.TraceIdentifier))
            using (var respStream = new MemoryStream())
            {
                var originalRespStream = context.Response.Body;
                
                try
                {
                    context.Response.Body = respStream;
                    var body = string.Empty;
                    
                    if (context.Request.Body.CanRead)
                    {
                        context.Request.EnableBuffering();
                        var stream = new StreamReader(context.Request.Body);
                        body = await stream.ReadToEndAsync();
                    }

                    var data = new
                    {
                        method = context.Request.Method,
                        scheme = context.Request.Scheme,
                        url = $"{context.Request.Scheme}{context.Request.Host}{context.Request.Path.ToString()}{context.Request.QueryString.ToString()}",
                        body
                    };

                    _logger.LogWithData(LogLevel.Debug,"Request log", data);
                }
                catch (Exception e)
                {
                    _logger.LogDebug(e, "failed to log request");
                }
                finally
                {
                    try
                    {
                        context.Request.Body.Position = 0;
                    }
                    catch (Exception)
                    {
                    }
                }

                await _next(context);

                try
                {
                    var body = string.Empty;

                    respStream.Position = 0;
                    using var sr = new StreamReader(respStream);
                    body = await sr.ReadToEndAsync();
                    respStream.Position = 0;

                    await respStream.CopyToAsync(originalRespStream);
                    context.Response.Body = originalRespStream;
                    
                    var data = new
                    {
                        body
                    };

                    _logger.LogWithData(LogLevel.Debug,"Response  log", data);
                }
                catch (Exception e)
                {
                    _logger.LogDebug(e, "failed to log request");
                }
            }
        }
    }
}