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

