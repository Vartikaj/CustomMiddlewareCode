using CustomMiddleWare.Interfaces;
using CustomMiddleWare.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;

namespace CustomMiddleWare.Middlewares
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class JwtMiddleware : AuthorizeAttribute
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public JwtMiddleware()
        {
        }

        // call by default

        // HttpContext : Used to read the data from the header
        // IRegistration : Used becuase we call function which took data from the database and save inside the token as a claim value
        // Authorization : Used because we call its function inside the class.
        public async Task Invoke(HttpContext httpContext, IRegistration registrationService, JwtUtils authorization)
        {
            var endpoint = httpContext.GetEndpoint();
            // ✅ Skip middleware logic if [AllowAnonymous] is applied
            if (endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() != null)
            {
                await _next(httpContext);
                return;
            }

            var token = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var userId = authorization.ValidateTokens(token);
                    if (userId != null)
                    {
                        var userRegistration = registrationService.GetHashCode(userId); // replace with your actual method
                        if (userRegistration != null)
                        {
                            httpContext.Items["Registration"] = userRegistration;
                        }
                    } else
                    {
                        httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        await httpContext.Response.WriteAsync("Invalid Token");
                        return;
                    }
                }
                catch (Exception)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await httpContext.Response.WriteAsync("Invalid Token");
                    return;
                }
            }

            await _next(httpContext);
            return;
        }
    }

    // this Extension method used to add the middleware to the HTTP request pipeline
    public static class JwtMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtMiddleware>();
        }
    }
}
