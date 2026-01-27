using MyPlatformInfrastructure.HttpClients;
using MyPlatformInfrastructure.Mapping;
using MyPlatformInfrastructure.Repositories;
using MyPlatformInfrastructure.Services;
using MyPlatformMcpServer.Configuration;
using MyPlatformMcpServer.Services;
using MyPlatformMcpServer.Tools;
using MyPlatformMcpServer.Tools.Statistics;
using MyPlatformModels;
using MyPlatformModels.HttpClients;
using MyPlatformModels.Repositories;
using MyPlatformModels.Services;
using McpServerInfrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IAgentIdRetrieverService, AgentIdRetrieverService>();

// In your Program.cs or startup code, before any MongoDB operations
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.CSharpLegacy));

builder.Services.Configure<AclConfiguration>(
    builder.Configuration.GetSection("AclConfiguration"));

builder.Services.AddHttpClient<IACLService, MockACLService>((svc, client) =>
{
    var aclConfig = svc.GetRequiredService<Microsoft.Extensions.Options.IOptions<AclConfiguration>>().Value;
    client.BaseAddress = new Uri(aclConfig.Endpoint);
    client.DefaultRequestHeaders.Add(aclConfig.ApiKeyHeaderName, aclConfig.ApiKeyValue);
});

// Configure FileServer HTTP client
builder.Services.Configure<FileServerConfiguration>(
    builder.Configuration.GetSection("FileServerConfiguration"));

builder.Services.AddHttpClient<IFileServerHttpClient, MockFileServerHttpClient>((svc, client) =>
{
    var fileServerConfig = svc.GetRequiredService<Microsoft.Extensions.Options.IOptions<FileServerConfiguration>>().Value;
    client.BaseAddress = new Uri(fileServerConfig.Endpoint);
    client.DefaultRequestHeaders.Add(fileServerConfig.ApiKeyHeaderName, fileServerConfig.ApiKeyValue);
});

// Configure AutoMapper - scan all loaded assemblies for profiles
builder.Services.AddAutoMapper(cfg => { }, 
    typeof(Program).Assembly, 
    typeof(FileMetadataMappingProfile).Assembly
);

// Register Repositories (Data Layer)
builder.Services.AddSingleton<IStatisticsRepository, MockStatisticsRepository>();
builder.Services.AddSingleton<ICompanyInfoRepository, MockCompanyInfoRepository>();

// Register Services (Business Logic Layer)
builder.Services.AddSingleton<ICompanyInfoService, CompanyInfoService>();
builder.Services.AddSingleton<IStatisticsService, StatisticsService>();
builder.Services.AddSingleton<IChartService, ChartService>();
builder.Services.AddSingleton<IFileServerService, FileServerService>();

var jwtConfig = new JwtConfiguration();
builder.Configuration.GetSection("Jwt").Bind(jwtConfig);

if(jwtConfig.Enabled)
{
    // Configure JWT Authentication with Keycloak
    var keycloakAuthority = jwtConfig.Issuer
        ?? throw new InvalidOperationException("JWT Issuer is required");
    var audience = jwtConfig.Audience
        ?? throw new InvalidOperationException("JWT Audience is required");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Keycloak provides the public keys via standard OAuth2 endpoint
        options.Authority = keycloakAuthority;  // e.g., "https://your-keycloak.com/realms/your-realm"
        options.Audience = audience;            // Your client ID or resource name
        options.RequireHttpsMetadata = false;   // Set to true in production

        options.TokenValidationParameters.ValidateIssuer = true;
        options.TokenValidationParameters.ValidateAudience = true;
        options.TokenValidationParameters.ValidateLifetime = true;
        options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(5);
    });

    builder.Services.AddAuthorization();
}

// Configure MCP Server
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<CompanyInfoTools>()
    .WithTools<PerformanceStatisticsTools>()
    .WithTools<StatisticsRatesTools>()
    .WithTools<MyPermissionsTool>()
    .WithTools<StatisticsTool>()
    .WithTools<ChartTools>();

builder.Logging.SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();

if (jwtConfig.Enabled)
{
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapMcp().RequireAuthorization();
} 
else
{
    app.MapMcp();
}

// k8s probes
app.MapGet("/health/liveness", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/health/readiness", () =>
{
    bool ready = true;
    return ready ? Results.Ok(new { status = "ready" })
                 : Results.StatusCode(503);
});


app.Run();