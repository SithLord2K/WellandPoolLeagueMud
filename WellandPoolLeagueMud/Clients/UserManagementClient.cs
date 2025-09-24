using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using WellandPoolLeagueMud.Data.ViewModels;

namespace WellandPoolLeagueMud.Clients;

public class UserManagementClient
{
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigationManager;

    public UserManagementClient(HttpClient httpClient, NavigationManager navigationManager)
    {
        _httpClient = httpClient;
        _navigationManager = navigationManager;
    }

    private void EnsureBaseAddress()
    {
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_navigationManager.BaseUri);
        }
    }

    public async Task<List<UserViewModel>?> GetUsersAsync()
    {
        EnsureBaseAddress();
        return await _httpClient.GetFromJsonAsync<List<UserViewModel>>("api/usermanagement/users");
    }

    public async Task<List<RoleViewModel>?> GetRolesAsync()
    {
        EnsureBaseAddress();
        return await _httpClient.GetFromJsonAsync<List<RoleViewModel>>("api/usermanagement/roles");
    }

    public async Task AssignRolesToUserAsync(string userId, List<string> roleIds)
    {
        EnsureBaseAddress();
        var response = await _httpClient.PostAsJsonAsync($"api/usermanagement/users/{userId}/roles", roleIds);
        response.EnsureSuccessStatusCode();
    }
}