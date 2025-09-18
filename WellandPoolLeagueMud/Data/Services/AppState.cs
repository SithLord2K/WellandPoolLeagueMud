using System.Security.Claims;

namespace WellandPoolLeagueMud.Services;

public class AppState
{
    public string? Username { get; private set; }
    public string? UserId { get; private set; }
    public string? Email { get; private set; }
    public string? Roles { get; private set; }
    public bool IsLoggedIn => !string.IsNullOrEmpty(Username);

    // Event to notify components that the state has changed
    public event Action? OnChange;

    public void SetUser(ClaimsPrincipal user)
    {
        if (user.Identity is not null && user.Identity.IsAuthenticated)
        {
            Username = user.Identity.Name;
            UserId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            Email = user.Claims.FirstOrDefault(c => c.Type == "https://wpl.codersden.com/email")?.Value;
            Roles = user.Claims.FirstOrDefault(c => c.Type == "https://wpl.codersden.com/roles")?.Value;
        }
        else
        {
            Username = null;
            UserId = null;
            Email = null;
            Roles = null;
        }

        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}