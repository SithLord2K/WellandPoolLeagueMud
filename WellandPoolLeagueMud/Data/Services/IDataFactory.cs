using WellandPoolLeagueMud.Data.Models;

namespace WellandPoolLeagueMud.Data.Services
{
    public interface IDataFactory
    {
        Task<List<Player>> GetPlayersAsync();
        Task<Player?> GetSinglePlayerAsync(int id);
        Task<bool> AddPlayerAsync(Player player);
        Task<bool> DeletePlayerAsync(int id);

        Task<List<PlayerDatum>> GetPlayerDatumAsync();
        Task<List<PlayerDatum>> GetSinglePlayerDataAsync(int playerId);
        Task<List<PlayerData>> GetAllPlayerDataAsync();
        Task<bool> SavePlayerDataAsync(PlayerDatum playerData);

        Task<List<PlayersView>> GetPlayersViewAsync();

        Task<List<TeamDetail>> GetTeamDetailsAsync();
        Task<TeamDetail?> GetSingleTeamAsync(int Id);
        Task<bool> AddTeamAsync(TeamDetail team);

        Task<List<Schedule>> GetSchedulesAsync();
        Task<Schedule?> GetSingleScheduleAsync(int Id);
        Task<bool> AddOrUpdateScheduleAsync(Schedule schedule);

        Task<List<Week>> GetAllWeeksAsync();
        Task<bool> AddWeeksAsync(Week weeks);
        Task<bool> UpdateWeeksAsync(Week weeks);
        Task RemoveWeeksAsync(int id);

        Task<List<WeeksView>> GetWeeksViewAsync();
    }
}