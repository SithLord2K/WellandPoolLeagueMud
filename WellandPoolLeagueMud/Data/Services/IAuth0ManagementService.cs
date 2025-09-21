using WellandPoolLeagueMud.ViewModels;

public interface IAuth0ManagementService
{
    Task<IEnumerable<UserViewModel>> GetUsersAsync();
    Task<IEnumerable<RoleViewModel>> GetRolesAsync();
    Task AssignRolesToUserAsync(string userId, List<string> roleIds);
}