using Auth0.ManagementApi.Models;
using WellandPoolLeagueMud.Data.ViewModels;

public interface IAuth0ManagementService
{
    Task<IEnumerable<UserViewModel>> GetUsersAsync();
    Task<IEnumerable<RoleViewModel>> GetRolesAsync();
    Task AssignRolesToUserAsync(string userId, List<string> roleIds);
    Task<User> CreateUserAsync(UserCreateRequest request);
    Task DeleteUserAsync(string userId);
    Task UpdateUserBlockAsync(string userId, bool isBlocked);
    Task CreateRoleAsync(RoleCreateRequest request);
    Task UpdateRoleAsync(string roleId, RoleUpdateRequest request);
    Task DeleteRoleAsync(string roleId);
}