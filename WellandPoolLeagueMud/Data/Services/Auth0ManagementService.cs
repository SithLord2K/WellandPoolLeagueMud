using Auth0.ManagementApi.Models;
using WellandPoolLeagueMud.Data.Services;
using WellandPoolLeagueMud.Data.ViewModels;

namespace WellandPoolLeagueMud.Data.Services
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

        /// <summary>
        /// Retrieves all users and their assigned roles from Auth0.
        /// </summary>
        public async Task<IEnumerable<UserViewModel>> GetUsersAsync()
        {
            try
            {
                var client = await _clientFactory.CreateClientAsync();

                // First, get all users from Auth0. We don't need to request the 'roles' field here.
                var usersFromAuth0 = await client.Users.GetAllAsync(new GetUsersRequest());
                var usersToReturn = new List<UserViewModel>();

                _logger.LogInformation("Retrieved {Count} users from Auth0. Now fetching roles for each user.", usersFromAuth0.Count);

                foreach (var user in usersFromAuth0)
                {
                    try
                    {
                        var roles = await client.Users.GetRolesAsync(user.UserId);

                        usersToReturn.Add(new UserViewModel
                        {
                            UserId = user.UserId,
                            Email = user.Email,
                            IsBlocked = user.Blocked ?? false,
                            Roles = roles.Select(r => r.Id).ToList()
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get roles for user {UserId}. The user will be listed without roles.", user.UserId);
                        usersToReturn.Add(new UserViewModel
                        {
                            UserId = user.UserId,
                            Email = user.Email,
                            IsBlocked = user.Blocked ?? false,
                            Roles = new List<string>()
                        });
                    }
                }

                _logger.LogInformation("Successfully processed {Count} users with their roles.", usersToReturn.Count);
                return usersToReturn;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A critical error occurred while getting users from Auth0.");
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
                    Name = role.Name,
                    Description = role.Description // <-- MAP THE DESCRIPTION
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

        // --- ADD THE THREE NEW METHODS BELOW ---

        public async Task CreateRoleAsync(RoleCreateRequest request)
        {
            try
            {
                var client = await _clientFactory.CreateClientAsync();
                _logger.LogInformation("Creating new role with name {RoleName}", request.Name);
                await client.Roles.CreateAsync(request);
                _logger.LogInformation("Successfully created role {RoleName}", request.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create role {RoleName}", request.Name);
                throw;
            }
        }

        public async Task UpdateRoleAsync(string roleId, RoleUpdateRequest request)
        {
            try
            {
                var client = await _clientFactory.CreateClientAsync();
                _logger.LogInformation("Updating role {RoleId}", roleId);
                await client.Roles.UpdateAsync(roleId, request);
                _logger.LogInformation("Successfully updated role {RoleId}", roleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update role {RoleId}", roleId);
                throw;
            }
        }

        public async Task DeleteRoleAsync(string roleId)
        {
            try
            {
                var client = await _clientFactory.CreateClientAsync();
                _logger.LogInformation("Deleting role {RoleId}", roleId);
                await client.Roles.DeleteAsync(roleId);
                _logger.LogInformation("Successfully deleted role {RoleId}", roleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete role {RoleId}", roleId);
                throw;
            }
        }

        public async Task AssignRolesToUserAsync(string userId, List<string> roleIds)
        {
            try
            {
                var client = await _clientFactory.CreateClientAsync();
                var currentUserRoles = await client.Users.GetRolesAsync(userId);
                var currentRoleIds = currentUserRoles.Select(r => r.Id).ToList();

                _logger.LogInformation("Updating roles for user {UserId}. Current: [{CurrentRoles}], New: [{NewRoles}]",
                    userId, string.Join(", ", currentRoleIds), string.Join(", ", roleIds));

                // Roles to remove are those in the current list but not in the new list.
                var rolesToRemove = currentRoleIds.Except(roleIds).ToArray();
                if (rolesToRemove.Any())
                {
                    await client.Users.RemoveRolesAsync(userId, new AssignRolesRequest { Roles = rolesToRemove });
                    _logger.LogDebug("Removed {Count} roles from user {UserId}", rolesToRemove.Length, userId);
                }

                // Roles to add are those in the new list but not in the current list.
                var rolesToAdd = roleIds.Except(currentRoleIds).ToArray();
                if (rolesToAdd.Any())
                {
                    await client.Users.AssignRolesAsync(userId, new AssignRolesRequest { Roles = rolesToAdd });
                    _logger.LogDebug("Assigned {Count} new roles to user {UserId}", rolesToAdd.Length, userId);
                }

                _logger.LogInformation("Successfully updated roles for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to assign roles to user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Creates a new user in Auth0.
        /// </summary>
        public async Task<User> CreateUserAsync(UserCreateRequest request)
        {
            try
            {
                var client = await _clientFactory.CreateClientAsync();
                _logger.LogInformation("Attempting to create a new user with email {Email}", request.Email);
                var newUser = await client.Users.CreateAsync(request);
                _logger.LogInformation("Successfully created user {UserId} with email {Email}", newUser.UserId, newUser.Email);
                return newUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user with email {Email}", request.Email);
                throw;
            }
        }

        /// <summary>
        /// Deletes a user from Auth0.
        /// </summary>
        public async Task DeleteUserAsync(string userId)
        {
            try
            {
                var client = await _clientFactory.CreateClientAsync();
                _logger.LogInformation("Attempting to delete user {UserId}", userId);
                await client.Users.DeleteAsync(userId);
                _logger.LogInformation("Successfully deleted user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Blocks or unblocks a user in Auth0.
        /// </summary>
        public async Task UpdateUserBlockAsync(string userId, bool isBlocked)
        {
            var action = isBlocked ? "Blocking" : "Unblocking";
            try
            {
                var client = await _clientFactory.CreateClientAsync();
                _logger.LogInformation("{Action} user {UserId}", action, userId);
                var request = new UserUpdateRequest { Blocked = isBlocked };
                await client.Users.UpdateAsync(userId, request);
                _logger.LogInformation("Successfully updated block status for user {UserId} to {IsBlocked}", userId, isBlocked);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed while {Action} user {UserId}", action, userId);
                throw;
            }
        }
    }
}