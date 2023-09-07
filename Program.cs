using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Security.Claims;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Add services to the container.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

    IConfiguration configuration = builder.Configuration;
    string loginServer = configuration["AppSettings:LoginServer"].ToString();
    string jwksEndpoint = configuration["AppSettings:JWKSEndpoint"].ToString();
    string validIssuer = configuration["AppSettings:ValidIssuer"].ToString();
    string validAudience = configuration["AppSettings:ValidAudience"].ToString();


    // In normal conditions, you should use the following code block. However;
    // Microsoft.AspNetCore.Authentication.JwtBearer could't validate the token while downloading the JWKS endpoint.
    // That's why we are using the code block below to download JWKS.json from the JWKS endpoint and validate the token.

    // options.Authority = loginServer;
    // options.MetadataAddress = jwksEndpoint;
    // options.RequireHttpsMetadata = true;
    // options.IncludeErrorDetails = true;
    // options.BackchannelHttpHandler = new HttpClientHandler
    //     {
    //         ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    //     };


    // Get the JWKS.json from the JWKS endpoint
    var httpClient = new HttpClient();
    var jwksResponse = httpClient.GetAsync(jwksEndpoint);
    var jwksJson = jwksResponse.Result.Content.ReadAsStringAsync().Result;

    JsonWebKeySet jsonWebKeySet = JsonConvert.DeserializeObject<JsonWebKeySet>(jwksJson);
    var key = jsonWebKeySet.Keys.First();

    // Configuring token validation parameters
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        IssuerSigningKey = key
    };

    // Adding Issuer validation if any
    if (validIssuer != null && validIssuer.Length > 0)
    {
        options.TokenValidationParameters.ValidateIssuer = true;
        options.TokenValidationParameters.ValidIssuer = validIssuer;
    }
    else
    {
        options.TokenValidationParameters.ValidateIssuer = false;
    }

    // Adding Audience validation if any
    if (validAudience != null && validAudience.Length > 0)
    {
        options.TokenValidationParameters.ValidateAudience = true;
        options.TokenValidationParameters.ValidAudience = validAudience;
    }
    else
    {
        options.TokenValidationParameters.ValidateAudience = false;
    }

    options.Events = new JwtBearerEvents()
    {
        OnTokenValidated = context =>
        {
            var user_id = context.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            logger.LogDebug("User authenticated: {user}", user_id);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            // Ensure we always have an error and error description.
            if (string.IsNullOrEmpty(context.Error))
                context.Error = "invalid_token";
            if (string.IsNullOrEmpty(context.ErrorDescription))
                context.ErrorDescription = "This request requires a valid JWT access token to be provided";

            // Add some extra context for expired tokens.
            if (context.AuthenticateFailure != null && context.AuthenticateFailure.GetType() == typeof(SecurityTokenExpiredException))
            {
                var authenticationException = context.AuthenticateFailure as SecurityTokenExpiredException;
                context.Response.Headers.Add("x-token-expired", authenticationException.Expires.ToString("o"));
                context.ErrorDescription = $"The token expired on {authenticationException.Expires.ToString("o")}";
            }

            return context.Response.WriteAsync(JsonConvert.SerializeObject(new
            {
                error = context.Error,
                error_description = context.ErrorDescription
            }));
        },
        OnAuthenticationFailed = context =>
        {
            // Custom logic here, be cautious about what you're logging or serializing
            return Task.CompletedTask;
        }
    };
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field. Don't forget to add 'Bearer ' before the token.",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
   {
     new OpenApiSecurityScheme
     {
       Reference = new OpenApiReference
       {
         Type = ReferenceType.SecurityScheme,
         Id = "Bearer"
       }
      },
      Array.Empty<string>()
    }
  });
});

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
