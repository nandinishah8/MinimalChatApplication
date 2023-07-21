using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
namespace MinimalChatApplication.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Save the original request body
            var originalRequestBody = context.Request.Body;

            try
            {
                // Read the request body
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    //context.Request.Body.Seek(0, SeekOrigin.Begin);
                    var requestBody = await reader.ReadToEndAsync();
                    context.Request.Body.Seek(0, SeekOrigin.Begin);

                    // Fetch IP address of the caller
                    var ipAddress = context.Connection.RemoteIpAddress?.ToString();

                    // Fetch username from auth token (Bearer token)
                    var username = "Unknown";
                    var authorizationHeader = context.Request.Headers["Authorization"].ToString();
                    if (authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        var token = authorizationHeader.Substring("Bearer ".Length).Trim();

                    }

                    // Log the request details
                    _logger.LogInformation($"Request IP: {ipAddress}, Username: {username}, Request Body: {requestBody}, Time of Call: {DateTime.UtcNow}");

                    // Continue processing the request
                    await _next(context);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging the request.");
                throw;
            }
            finally
            {
                // Restore the original request body
                context.Request.Body = originalRequestBody;
            }
        }

            private string ExtractUsernameFromToken(string token)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

               
                var username = jwtToken.Subject;

                return username;
            }




        }
    }

