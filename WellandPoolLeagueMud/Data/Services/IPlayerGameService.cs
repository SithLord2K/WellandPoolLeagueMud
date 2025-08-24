using WellandPoolLeagueMud.Data.ViewModels;

namespace WellandPoolLeagueMud.Data.Services
{
    public interface IPlayerGameService
    {
        Task<List<PlayerGameViewModel>> GetAllPlayerGamesAsync();
        Task<PlayerGameViewModel?> GetPlayerGameByIdAsync(int id);
        Task<PlayerGameViewModel> CreatePlayerGameAsync(PlayerGameViewModel playerGame);
        Task<PlayerGameViewModel?> UpdatePlayerGameAsync(PlayerGameViewModel playerGame);
        Task<bool> DeletePlayerGameAsync(int id);
        Task<List<PlayerGameViewModel>> GetPlayerGamesByWeekAsync(int weekNumber);
        Task<List<PlayerGameViewModel>> GetPlayerGamesByPlayerAsync(int playerId);
        Task<List<PlayerGameViewModel>> GetPlayerGamesByTeamAsync(int teamId);
    }
}