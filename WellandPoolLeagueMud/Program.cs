using Auth0.AspNetCore.Authentication;
using Auth0.ManagementApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WellandPoolLeagueMud.AuthenticationStateSyncer.PersistingRevalidatingAuthenticationStateProvider;
using WellandPoolLeagueMud.Clients;
using WellandPoolLeagueMud.Components;
using WellandPoolLeagueMud.Data;
using WellandPoolLeagueMud.Data.Services;
using WellandPoolLeagueMud.Handlers;
using WellandPoolLeagueMud.Reports;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

QuestPDF.Settings.License = LicenseType.Community;

try
{
    Log.Information("Starting up the application");
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddScoped<IPlayerService, PlayerService>();
    builder.Services.AddScoped<ITeamService, TeamService>();
    builder.Services.AddScoped<IPlayerGameService, PlayerGameService>();
    builder.Services.AddScoped<IScheduleService, ScheduleService>();
    builder.Services.AddScoped<AppState>();
    builder.Services.AddScoped<IUserProfileService, UserProfileService>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddTransient<CookieForwardingHandler>();
    builder.Services.AddScoped<IPlayerUserService, PlayerUserService>();
    builder.Services.AddScoped<IScheduleGeneratorService, ScheduleGeneratorService>();
    builder.Services.AddScoped<IBarService, BarService>();
    builder.Services.AddScoped<WellandPoolLeagueMud.Data.Services.DialogService>();

    builder.Services.AddDbContextFactory<WellandPoolLeagueDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("WPLStatsDB"));
    });

    var keysFolder = Path.Combine(builder.Environment.ContentRootPath, "..", "keys");
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keysFolder))
        .SetApplicationName("WellandPoolLeague");

    builder.Services.Configure<Auth0Settings>(builder.Configuration.GetSection(Auth0Settings.SectionName));
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<IAuth0TokenService, Auth0TokenService>();
    builder.Services.AddScoped<IAuth0ManagementClientFactory, Auth0ManagementClientFactory>();
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

    builder.Services.Configure<OpenIdConnectOptions>(Auth0Constants.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters.RoleClaimType = "https://wpl.codersden.com/roles";

        options.Events.OnTicketReceived = (context) =>
        {
            const string namespaceUrl = "https://wpl.codersden.com/";
            const string createdAtClaimName = $"{namespaceUrl}created_at";

            var idToken = context.Properties?.GetTokenValue("id_token");
            if (idToken != null)
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(idToken);
                var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;

                var createdAtClaim = token.Claims.FirstOrDefault(c => c.Type == createdAtClaimName);
                if (createdAtClaim != null && !claimsIdentity!.HasClaim(c => c.Type == createdAtClaim.Type))
                {
                    claimsIdentity?.AddClaim(new Claim(createdAtClaim.Type, createdAtClaim.Value));
                }

                const string rolesClaimName = $"{namespaceUrl}roles";
                var rolesClaims = token.Claims.Where(c => c.Type == rolesClaimName);

                foreach (var roleClaim in rolesClaims)
                {
                    claimsIdentity?.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));
                }
            }

            return Task.CompletedTask;
        };
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

    app.MapGet("/Account/ChangePassword", async (HttpContext httpContext) =>
    {
        var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
            .WithRedirectUri("/profile")
            .Build();

        await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    var reportsApi = app.MapGroup("api/reports")
                         .RequireAuthorization();

    reportsApi.MapGet("teamstandings", async (ITeamService teamService, IWebHostEnvironment env) =>
    {
        var logoPath = Path.Combine(env.WebRootPath, "images", "appicon.png");
        byte[] logoData = await File.ReadAllBytesAsync(logoPath);
        var standings = await teamService.GetTeamStandingsAsync();
        var report = new TeamStandingsReport(standings, logoData);
        byte[] pdfBytes = report.GeneratePdf();
        return Results.File(pdfBytes, "application/pdf", "TeamStandingsReport.pdf");
    });

    reportsApi.MapGet("weeklyresults/{weekNumber:int}", async (int weekNumber, IScheduleService scheduleService, IWebHostEnvironment env) =>
    {
        var logoPath = Path.Combine(env.WebRootPath, "images", "appicon.png");
        byte[] logoData = await File.ReadAllBytesAsync(logoPath);
        var results = await scheduleService.GetSchedulesByWeekAsync(weekNumber);
        var report = new WeeklyResultsReport(results, weekNumber, logoData);
        byte[] pdfBytes = report.GeneratePdf();
        return Results.File(pdfBytes, "application/pdf", $"Week_{weekNumber}_Results.pdf");
    });

    reportsApi.MapGet("playerstandings", async (IPlayerService playerService, IWebHostEnvironment env) =>
    {
        var logoPath = Path.Combine(env.WebRootPath, "images", "appicon.png");
        byte[] logoData = await File.ReadAllBytesAsync(logoPath);
        var standings = await playerService.GetPlayerStandingsAsync();
        var report = new PlayerStandingsReport(standings, logoData);
        byte[] pdfBytes = report.GeneratePdf();
        return Results.File(pdfBytes, "application/pdf", "PlayerStandingsReport.pdf");
    });

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