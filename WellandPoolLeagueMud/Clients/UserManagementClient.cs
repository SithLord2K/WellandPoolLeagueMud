using WellandPoolLeagueMud.ViewModels; // Make sure this using statement matches your shared project

namespace WellandPoolLeagueMud.Clients
{

    public class UserManagementClient
    {
        private readonly HttpClient _httpClient;

        // The configured HttpClient is injected here by the dependency injection container
        public UserManagementClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Calls the backend API to get a list of all users.
        /// </summary>
        public async Task<List<UserViewModel>?> GetUsersAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<UserViewModel>>("api/usermanagement/users");
        }

        /// <summary>
        /// Calls the backend API to get a list of all available roles.
        /// </summary>
        public async Task<List<RoleViewModel>?> GetRolesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<RoleViewModel>>("api/usermanagement/roles");
        }

        /// <summary>
        /// Calls the backend API to assign a list of roles to a specific user.
        /// </summary>
        public async Task AssignRolesToUserAsync(string userId, List<string> roleIds)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/usermanagement/users/{userId}/roles", roleIds);

            // This will throw an exception if the API returns an error (e.g., 401 Unauthorized)
            response.EnsureSuccessStatusCode();
        }
    }
}