using CustomMiddleWare.Interfaces;
using CustomMiddleWare.Middlewares;
using CustomMiddleWare.Services;
using CustomMiddleWare.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using StackExchange.Redis;
using System.Data;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Register your registration service
builder.Services.AddScoped<IRegistration, RegistrationService>();
// builder.Services.AddScoped<JwtUtils>();
// Register the MySQL DB connection for Dapper

builder.Services.AddScoped<IDbConnection>(sp =>
    new MySqlConnection(builder.Configuration.GetConnectionString("Connection")));

// redis cache
builder.Services.AddStackExchangeRedisCache(sp =>
{
    sp.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
});

// add middleware
//builder.Services.AddAuthentication("Bearer")
//    .AddJwtBearer("Bearer", options =>
//    {
//        options.Authority = "http://localhost:5183"; // IdentityServer URL
//        options.RequireHttpsMetadata = false;
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateAudience = true,
//            ValidAudience = "CustomMiddleWare", // must match 'aud' in token
//            ClockSkew = TimeSpan.Zero
//        };
//    });

// Add services
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie("Cookies")
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = "https://your-identityserver-url"; // e.g. https://localhost:5001
    options.ClientId = "mvc";
    options.ClientSecret = "secret";
    options.ResponseType = "code";

    // 🔽 Clear default scopes and add OpenID scopes
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("CustomMiddleWare.write"); // your API scope
    options.Scope.Add("offline_access"); // optional, for refresh token

    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;

    // Optional
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name",
        RoleClaimType = "role"
    };
});


builder.Services.AddControllers();
builder.Services.AddScoped<CacheService>();

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
//}).AddJwtBearer(o =>
//{
//    o.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidIssuer = builder.Configuration["Jwt:Issuer"],
//        ValidAudience = builder.Configuration["Jwt:Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
//        ValidateAudience = true,
//        ValidateIssuer = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true
//    };
//});

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();
