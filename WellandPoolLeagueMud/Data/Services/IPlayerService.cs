using WellandPoolLeagueMud.Data.ViewModels;

namespace WellandPoolLeagueMud.Data.Services
{
    public interface IPlayerService
    {
        Task<List<PlayerViewModel>> GetAllPlayersAsync();
        Task<PlayerViewModel?> GetPlayerByIdAsync(int id);
        Task<PlayerViewModel> CreatePlayerAsync(PlayerViewModel player);
        Task<PlayerViewModel?> UpdatePlayerAsync(PlayerViewModel player);
        Task<bool> DeletePlayerAsync(int id);
        Task<List<PlayerStandingViewModel>> GetPlayerStandingsAsync();
        Task<bool> PlayerExistsAsync(int id);
        Task<List<PlayerViewModel>> GetPlayersByTeamAsync(int teamId);
        Task<UserViewModel> GetPlayerProfileAsync(string auth0UserId);
    }
}