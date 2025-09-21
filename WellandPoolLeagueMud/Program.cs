using Serilog;
using Auth0.AspNetCore.Authentication;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using WellandPoolLeagueMud.Handlers;
using WellandPoolLeagueMud.AuthenticationStateSyncer.PersistingRevalidatingAuthenticationStateProvider;
using WellandPoolLeagueMud.Clients;
using WellandPoolLeagueMud.Components;
using WellandPoolLeagueMud.Data;
using WellandPoolLeagueMud.Data.Services;
using WellandPoolLeagueMud.Services;
using Microsoft.AspNetCore.DataProtection;

// --- 1. Configure Serilog Bootstrap Logger ---
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting up the application");
    var builder = WebApplication.CreateBuilder(args);

    // --- 2. Add Serilog to the Host Builder ---
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // --- Your Existing Service Registrations ---
    builder.Services.AddScoped<IPlayerService, PlayerService>();
    builder.Services.AddScoped<ITeamService, TeamService>();
    builder.Services.AddScoped<IPlayerGameService, PlayerGameService>();
    builder.Services.AddScoped<IScheduleService, ScheduleService>();
    builder.Services.AddScoped<AppState>();
    builder.Services.AddScoped<IUserProfileService, UserProfileService>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddTransient<CookieForwardingHandler>();

    builder.Services.AddDbContextFactory<WellandPoolLeagueDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("WPLStatsDB"));
    });

    var keysFolder = Path.Combine(builder.Environment.ContentRootPath, "..", "keys");
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keysFolder))
        .SetApplicationName("WellandPoolLeague");

    // --- Auth0 Configuration and Services ---
    // Configure Auth0 settings
    builder.Services.Configure<Auth0Settings>(builder.Configuration.GetSection(Auth0Settings.SectionName));

    // Add memory cache for token caching
    builder.Services.AddMemoryCache();

    // Register Auth0 token service and factory
    builder.Services.AddSingleton<IAuth0TokenService, Auth0TokenService>();
    builder.Services.AddScoped<IAuth0ManagementClientFactory, Auth0ManagementClientFactory>();

    // Register ManagementApiClient using the factory (deferred creation)
    builder.Services.AddScoped<IManagementApiClient>(sp =>
    {
        var tokenService = sp.GetRequiredService<IAuth0TokenService>();
        var config = sp.GetRequiredService<IConfiguration>();
        var domain = config["Auth0:Domain"];

        var logger = sp.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Creating ManagementApiClient for domain: {Domain}", domain);

        try
        {
            var token = tokenService.GetManagementApiTokenAsync().GetAwaiter().GetResult();
            return new ManagementApiClient(token, domain!);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create ManagementApiClient");
            throw;
        }
    });

    builder.Services.AddScoped<IAuth0ManagementService, Auth0ManagementService>();

    builder.Services.AddHttpClient<UserManagementClient>()
        .AddHttpMessageHandler<CookieForwardingHandler>();

    builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(UserManagementClient)));

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddAuth0WebAppAuthentication(options =>
    {
        options.Domain = builder.Configuration.GetValue<string>("Auth0:Domain")!;
        options.ClientId = builder.Configuration.GetValue<string>("Auth0:ClientId")!;
    });

    builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddScoped<AntiForgery>();
    builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();
    builder.Services.AddMudServices();

    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("Super_User", policy =>
            policy.RequireRole("Super_User"));
    });

    var app = builder.Build();

    // --- 3. Add Serilog Request Logging Middleware ---
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAntiforgery();

    app.MapGet("/Account/Login", async (HttpContext httpContext, string returnUrl = "/") =>
    {
        var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
            .WithRedirectUri(returnUrl)
            .Build();
        await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
    });

    app.MapGet("/Account/Logout", async (HttpContext httpContext) =>
    {
        var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
            .WithRedirectUri("/")
            .Build();
        await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    var api = app.MapGroup("api/usermanagement")
                 .RequireAuthorization("Super_User");

    api.MapGet("users", async (IAuth0ManagementService service) =>
    {
        var users = await service.GetUsersAsync();
        return Results.Ok(users);
    });

    api.MapGet("roles", async (IAuth0ManagementService service) =>
    {
        var roles = await service.GetRolesAsync();
        return Results.Ok(roles);
    });

    api.MapPost("users/{userId}/roles", async (string userId, List<string> roleIds, IAuth0ManagementService service) =>
    {
        await service.AssignRolesToUserAsync(userId, roleIds);
        return Results.NoContent();
    });

    app.MapGet("/health/auth0", async (IAuth0TokenService tokenService, ILogger<Program> logger) =>
    {
        try
        {
            var token = await tokenService.GetManagementApiTokenAsync();
            logger.LogInformation("Auth0 health check successful");
            return Results.Ok(new { status = "healthy", hasToken = !string.IsNullOrEmpty(token) });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Auth0 health check failed");
            return Results.Problem($"Auth0 unhealthy: {ex.Message}");
        }
    }).AllowAnonymous();

    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}