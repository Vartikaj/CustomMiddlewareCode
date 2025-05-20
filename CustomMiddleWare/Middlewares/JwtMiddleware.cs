using CustomMiddleWare.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
       

        private IConfiguration _configuration;

        public JwtMiddleware(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // call by default

        // HttpContext : Used to read the data from the header
        // IRegistration : Used becuase we call function which took data from the database and save inside the token as a claim value
        // Authorization : Used because we call its function inside the class.
        public async Task Invoke(HttpContext httpContext, IRegistration registrationService, JwtUtiles authorization)
        {
            var token = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null)
            {
                throw new InvalidOperationException();
            }

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var userId = authorization.ValidateToken(token);
                    if(userId != null) {
                        var userRegistration = registrationService.GetHashCode(userId);
                        if(userRegistration != null)
                        {
                            httpContext.Items["Registration"] = userRegistration;
                            return;
                        }
                    }
                } catch (Exception ex)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                }
            }
        }

        public string? ValidateToken(string token)
        {
            if (token == null)
            {
                return null;
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                };

                SecurityToken validateToken;
                tokenHandler.ValidateToken(token, validationParameters, out validateToken);
                var jwtToken = (JwtSecurityToken)validateToken;

                var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "email");
                if(userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                {
                    return userId.ToString();
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
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
