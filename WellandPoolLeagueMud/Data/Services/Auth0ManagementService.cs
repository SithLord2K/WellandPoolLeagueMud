using Auth0.ManagementApi.Models;
using WellandPoolLeagueMud.Data.Services;
using WellandPoolLeagueMud.ViewModels;

namespace WellandPoolLeagueMud.Services
{
    public class Auth0ManagementService : IAuth0ManagementService
    {
        private readonly IAuth0ManagementClientFactory _clientFactory;
        private readonly ILogger<Auth0ManagementService> _logger;

        public Auth0ManagementService(
            IAuth0ManagementClientFactory clientFactory,
            ILogger<Auth0ManagementService> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<UserViewModel>> GetUsersAsync()
        {
            try
            {
                var client = await _clientFactory.CreateClientAsync();
                var usersFromAuth0 = await client.Users.GetAllAsync(new GetUsersRequest());
                var usersToReturn = new List<UserViewModel>();

                _logger.LogInformation("Retrieved {Count} users from Auth0, fetching roles for each user", usersFromAuth0.Count);

                foreach (var user in usersFromAuth0)
                {
                    try
                    {
                        var roles = await client.Users.GetRolesAsync(user.UserId);
                        usersToReturn.Add(new UserViewModel
                        {
                            UserId = user.UserId,
                            Email = user.Email,
                            Roles = roles.Select(r => r.Id).ToList()
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get roles for user {UserId}, adding user without roles", user.UserId);
                        // Add user without roles if role retrieval fails
                        usersToReturn.Add(new UserViewModel
                        {
                            UserId = user.UserId,
                            Email = user.Email,
                            Roles = new List<string>()
                        });
                    }
                }

                _logger.LogInformation("Successfully processed {Count} users with their roles", usersToReturn.Count);
                return usersToReturn;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get users from Auth0");
                throw;
            }
        }

        public async Task<IEnumerable<RoleViewModel>> GetRolesAsync()
        {
            try
            {
                var client = await _clientFactory.CreateClientAsync();
                var rolesFromAuth0 = await client.Roles.GetAllAsync(new GetRolesRequest());

                var roles = rolesFromAuth0.Select(role => new RoleViewModel
                {
                    Id = role.Id,
                    Name = role.Name
                }).ToList();

                _logger.LogInformation("Successfully retrieved {Count} roles from Auth0", roles.Count);
                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get roles from Auth0");
                throw;
            }
        }

        public async Task AssignRolesToUserAsync(string userId, List<string> newRoleIds)
        {
            try
            {
                var client = await _clientFactory.CreateClientAsync();
                var currentUserRoles = await client.Users.GetRolesAsync(userId);
                var currentRoleIds = currentUserRoles.Select(r => r.Id).ToList();

                _logger.LogInformation("Updating roles for user {UserId}. Current roles: {CurrentRoles}, New roles: {NewRoles}",
                    userId, string.Join(", ", currentRoleIds), string.Join(", ", newRoleIds));

                // Remove current roles if any exist
                if (currentRoleIds.Any())
                {
                    var removeRolesRequest = new AssignRolesRequest
                    {
                        Roles = currentRoleIds.ToArray()
                    };
                    await client.Users.RemoveRolesAsync(userId, removeRolesRequest);
                    _logger.LogDebug("Removed {Count} existing roles from user {UserId}", currentRoleIds.Count, userId);
                }

                // Assign new roles if any provided
                if (newRoleIds.Any())
                {
                    var assignRolesRequest = new AssignRolesRequest
                    {
                        Roles = newRoleIds.ToArray()
                    };
                    await client.Users.AssignRolesAsync(userId, assignRolesRequest);
                    _logger.LogDebug("Assigned {Count} new roles to user {UserId}", newRoleIds.Count, userId);
                }

                _logger.LogInformation("Successfully updated roles for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to assign roles to user {UserId}", userId);
                throw;
            }
        }
    }
}