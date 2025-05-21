using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace CustomMiddleWare.Utilities
{
    public class JwtUtils
    {
        private IConfiguration _config;

        public JwtUtils(IConfiguration config)
        {
            _config = config;
        }

        public string? ValidateTokens(string token)
        {
            if (token == null)
                return null;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                };

                SecurityToken validateToken;
                tokenHandler.ValidateToken(token, validationParameters, out validateToken);

                var jwtToken = (JwtSecurityToken)validateToken;

                var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "id");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
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
}
