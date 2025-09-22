using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WellandPoolLeagueMud.Data;
using WellandPoolLeagueMud.Data.Models;

namespace WellandPoolLeagueMud.Data.Services;

public class UserProfileService : IUserProfileService
{
    private readonly WellandPoolLeagueDbContext _context;

    public UserProfileService(WellandPoolLeagueDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfile?> GetByAuth0IdAsync(string auth0Id)
    {
        return await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.Auth0UserId == auth0Id);
    }

    public async Task<UserProfile> GetOrCreateUserProfileAsync(ClaimsPrincipal user)
    {
        var auth0Id = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(auth0Id))
        {
            throw new InvalidOperationException("Cannot find user ID from ClaimsPrincipal.");
        }

        var userProfile = await GetByAuth0IdAsync(auth0Id);

        if (userProfile == null)
        {
            // Use standard ClaimTypes for better maintainability
            var firstName = user.FindFirstValue(ClaimTypes.GivenName);
            var lastName = user.FindFirstValue(ClaimTypes.Surname);

            // Fallback logic to parse the full name if specific claims aren't present
            if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName))
            {
                var fullName = user.Identity?.Name;
                if (!string.IsNullOrWhiteSpace(fullName))
                {
                    int firstSpaceIndex = fullName.IndexOf(' ');
                    if (firstSpaceIndex == -1)
                    {
                        firstName = fullName;
                        lastName = "";
                    }
                    else
                    {
                        firstName = fullName.Substring(0, firstSpaceIndex);
                        lastName = fullName.Substring(firstSpaceIndex + 1);
                    }
                }
            }

            userProfile = new UserProfile
            {
                Auth0UserId = auth0Id,
                FirstName = firstName ?? "New",
                LastName = lastName ?? "User",

            };

            _context.UserProfiles.Add(userProfile);
            await _context.SaveChangesAsync();
        }

        return userProfile;
    }

    public async Task UpdateUserProfileAsync(UserProfile profile)
    {
        _context.UserProfiles.Update(profile);
        await _context.SaveChangesAsync();
    }
}