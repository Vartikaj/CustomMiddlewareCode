<h1>Middleware</h1>
Middleware is a kind of software component. In .NET web API applications middleware is placed in between the server and the application. Middleware can be used for several purposes such as authentication, logging, exception handling, etc.

<h3>Scenario</h3>
A platform should be implemented to publish small stories about fairy tales. Users who registered with the system can publish their own stories and able to view all available stories in the system. Users should be able to register with the system and should be able to log in to the system using their username and password.

This application will be implemented in accordance with the above-mentioned scenario.


<h3>Request Flow Architecture</h3>
<br/>
<img src="https://github.com/user-attachments/assets/3ec567d5-c871-4588-b099-05bec55aa13c" alt= "Middleware"/>

<br/>
<h3>Middleware in ASP.NET Core: Key Concepts</h3>
<ol>
  <li>
    <h4>
      RequestDelegate
    </h4>
    <p>Middleware is a delegate that takes an HttpContext object and returns a Task. The RequestDelegate represents the next middleware in the pipeline, allowing each component to either handle the request or pass it along.</p>
  </li>
   <li>
    <h4>
      HttpContext
    </h4>
    <p>The HttpContext object contains information about the current HTTP request and response, including headers, body, query parameters, user information, etc. Middleware components interact with this context to manipulate the flow.</p>
  </li>
</ol>

<h3>Code Working: </h3>


<h4><b>Invoke</b> :</h4> 
In ASP.NET Core middleware, the standard method <b>signature</b> looks like:
<b>public async Task Invoke(HttpContext context)</b>
But if you want to use Dependency Injection (DI) to access services like IRegistration or JwtUtils, ASP.NET Core allows you to add additional parameters to the Invoke method.
This method has 3 parameters:
<ol>
  <li>
    HttpContext httpContext – required for all middleware (represents the current request/response).
  </li>
  <li>
    IRegistration registrationService – DI injected service to interact with registration data.
  </li>
  <li>
    JwtUtils authorization – DI injected utility class you created for JWT token handling.
  </li>
</ol>

<h5>Workflow : </h5>
First calling Invoke function this will check the conteoller having [AllowAnonymous] filter or not. All the information about conteoller we get from the HttpContext.
using <pre>endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() != null</pre> we check is it contain filter or not.
If it contain then call the controller first whithout checking the Authorization.
If it not contain AllowAnonmous. then took Authentication form the header using httpContext and took the second text after space. 
Then check <pre>if (!string.IsNullOrEmpty(token))</pre>. Check token is not or Empty.

<h5>ValidateToken : </h5>
<pre>
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
            tokenHandler.ValidateToken(token, validationParameters, out validateToken); //out function is a call by reference funtion which is used to overite the validateToken value if new token is validate. and automatically pass. It is not mandatory out parametername present in the same class.

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
</pre>

If token is not null the validate the token by calling <b>ValidateTokens</b> method if all the claims inside the token is satisfying the condition the this will return us the id used inside the token. and the we pass this Id which we get from the Validation token then we go to the registration service and get all the data present inside the database.

<h5>Registration Service : </h5>
<pre>
  public async Task<ResultModel<RegistrationModel>> GetHashCode(string id)
  {
      ResultModel<RegistrationModel> result = new ResultModel<RegistrationModel>();
      try
      {
          if (id != null)
          {
              var sql = "SELECT * FROM registration WHERE id = @id";
              DynamicParameters dynamicParameters = new DynamicParameters();
              dynamicParameters.Add("id", id);
              var count = await _connection.QueryAsync<RegistrationModel>(sql, dynamicParameters);
              var countData = await _connection.QueryFirstOrDefaultAsync(sql, dynamicParameters);
              if(countData > 0)
              {
                  result.success = countData > 0;
                  result.LstModel = count.ToList();
              }
          }
      }
      catch (Exception ex)
      {
          throw ex;
      }
      return result;
  }
</pre>
using the above code will return all the record present inside the database.

<pre>
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
}</pre>

<pre>private readonly RequestDelegate _next;

public JwtMiddleware(RequestDelegate next)
{
    _next = next;
}</pre>

RequestDelegate should be added because it is responsible to call the next pipeline and in the invoke function we need to add <pre>await _next(httpContext);</pre> to call the next middleware in the pipeline.
and also we need to add the [Authorize] filter above the controller which need authentication before calling it.

For calling this pipeline during the code load we need to use Extension class. 
<pre>
  // this Extension method used to add the middleware to the HTTP request pipeline
  public static class JwtMiddlewareExtensions
  {
      public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder builder)
      {
          return builder.UseMiddleware<JwtMiddleware>(); // this will add the middleware in the program.cs file
      }
  }
</pre>

<h4>Program.cs</h4>

This is the file which load first in during the application start executing.

<pre>
  builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o=>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddAuthorization();
</pre>

this code need to to added because If you do not include the AddAuthentication() and AddJwtBearer() setup in your Program.cs, JWT token validation will not happen automatically, and you'll face the following issues:
<ol>
  <li>🔴 [Authorize] or [JwtMiddleware] won’t work properly
These attributes rely on the authentication scheme being set up correctly. Without it:
The request will always be unauthenticated.
Your controller action may still get executed if custom middleware doesn't block it.</li>
  <li>🔴 User.Identity and User.Claims will be empty
The built-in middleware that decodes JWT tokens and sets the HttpContext.User won’t run.</li>
  <li>🔴 You will have to manually validate and parse the JWT token (which you're doing in your JwtMiddleware) — but it won't fully integrate with ASP.NET Core's Authorization system.</li>
</ol>

<h4>✅ What Happens When You DO Include It</h4>
The ASP.NET Core Authentication middleware:
<ol>
  <li>Reads the Authorization: Bearer <token> header</li>
  <li>Validates the token using the configured TokenValidationParameters</li>
  <li>Sets HttpContext.User with claims from the token</li>
  <li>[Authorize] attributes work out of the box</li>
  <li>You don’t need to manually parse the JWT in middleware unless you want to add custom logic</li>
</ol>
