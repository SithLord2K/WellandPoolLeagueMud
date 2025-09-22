using System.Security.Claims;

namespace WellandPoolLeagueMud.Services;

public class AppState
{
    public string? Username { get; private set; }
    public string? UserId { get; private set; }
    public string? Email { get; private set; }
    public List<string> Roles { get; private set; } = new();
    public DateTime? RegistrationDate { get; private set; }
    public bool IsLoggedIn => !string.IsNullOrEmpty(UserId);

    public event Action? OnChange;

    public void SetUser(ClaimsPrincipal user)
    {
        if (user.Identity is not null && user.Identity.IsAuthenticated)
        {
            Username = user.Identity.Name;
            UserId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            Email = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            Roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            var registrationDateClaim = user.FindFirst("https://wpl.codersden.com/created_at")?.Value;
            if (!string.IsNullOrEmpty(registrationDateClaim))
            {
                RegistrationDate = DateTime.Parse(registrationDateClaim, null, System.Globalization.DateTimeStyles.RoundtripKind);
            }
        }
        else
        {
            Username = null;
            UserId = null;
            Email = null;
            Roles.Clear();
            RegistrationDate = null;
        }

        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}