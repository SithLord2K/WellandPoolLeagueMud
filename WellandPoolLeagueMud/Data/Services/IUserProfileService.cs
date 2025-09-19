using System.Security.Claims;
using WellandPoolLeagueMud.Data.Models;

namespace WellandPoolLeagueMud.Data.Services;

public interface IUserProfileService
{
    /// <summary>
    /// Gets a user profile from your local database based on the Auth0 User ID.
    /// </summary>
    /// <param name="auth0Id">The unique user ID from Auth0.</param>
    /// <returns>The user's profile, or null if not found.</returns>
    Task<UserProfile?> GetByAuth0IdAsync(string auth0Id);

    /// <summary>
    /// Updates an existing user profile in the database.
    /// </summary>
    /// <param name="profile">The user profile with updated information.</param>
    Task UpdateUserProfileAsync(UserProfile profile);

    /// <summary>
    /// Checks if a user profile exists and creates one if it doesn't ("Just-in-Time Provisioning").
    /// </summary>
    /// <param name="user">The ClaimsPrincipal for the currently authenticated user.</param>
    /// <returns>The existing or newly created user profile.</returns>
    Task<UserProfile> GetOrCreateUserProfileAsync(ClaimsPrincipal user);
}