using Microsoft.AspNetCore.Components.Authorization;

namespace WellandPoolLeagueMud.Data.Services
{
    public class AntiForgery
    {
        public async Task<bool> IsUserAuthenticated(AuthenticationStateProvider authStateProvider)
        {
            var authState = await authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            return user.Identity is not null && user.Identity.IsAuthenticated;
        }
    }
}