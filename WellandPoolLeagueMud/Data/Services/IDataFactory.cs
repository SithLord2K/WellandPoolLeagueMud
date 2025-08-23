using WellandPoolLeagueMud.Data.Models;

namespace WellandPoolLeagueMud.Data.Services
{
    public interface IDataFactory
    {
        // Players
        Task<List<WPL_Player>> GetPlayersAsync();
        Task<WPL_Player?> GetSinglePlayerAsync(int id);
        Task<bool> SavePlayerAsync(WPL_Player player);
        Task<bool> DeletePlayerAsync(int id);

        // Teams
        Task<List<WPL_Team>> GetTeamsAsync();
        Task<WPL_Team?> GetSingleTeamAsync(int Id);
        Task<bool> SaveTeamAsync(WPL_Team team);
        Task<bool> DeleteTeamAsync(int id);

        // Schedules
        Task<List<WPL_Schedule>> GetSchedulesAsync();
        Task<WPL_Schedule?> GetSingleScheduleAsync(int Id);
        Task<bool> AddOrUpdateScheduleAsync(WPL_Schedule schedule);
        Task<bool> DeleteScheduleAsync(int id);

        // Player Games
        Task<List<WPL_PlayerGame>> GetAllPlayerGamesAsync();
        Task<List<WPL_PlayerGame>> GetPlayerGamesByPlayerIdAsync(int playerId);
        Task<bool> SavePlayerGameAsync(WPL_PlayerGame playerGame);
        Task<bool> DeletePlayerGameAsync(int id);

        // Weekly Winners
        Task<List<WPL_WeeklyWinner>> GetWeeklyWinnersAsync();
        Task<WPL_WeeklyWinner?> GetSingleWeeklyWinnerAsync(int id);
        Task<bool> AddOrUpdateWeeklyWinnerAsync(WPL_WeeklyWinner weeklyWinner);
        Task<bool> DeleteWeeklyWinnerAsync(int id);
    }
}