using WellandPoolLeagueMud.Data.Models;

namespace WellandPoolLeagueMud.Data.Services
{
    public interface IDataFactory
    {
        Task<List<Player>> GetPlayers();
        Task<List<PlayerDatum>> GetPlayerData();
        Task<List<PlayersView>> GetPlayersView();
        Task<Player?> GetSinglePlayer(int id);
        Task<List<PlayerDatum>> GetSinglePlayerData(int playerId);
        Task<List<PlayerDatum>> GetAllPlayerData();
        Task<bool> AddPlayer(Player player);
        Task<bool> SavePlayerData(PlayerDatum playerData);
        Task<bool> DeletePlayer(int id);
        Task<List<Schedule>> GetSchedule();
        Task<Schedule> GetSingleSchedule(int Id);
        Task<bool> AddSchedule(Schedule schedule);
        Task<List<TeamDetail>> GetTeamDetails();
        Task<TeamDetail> GetSingleTeam(int Id);
        Task<bool> AddTeam(TeamDetail team);
        Task<List<Week>> GetAllWeeks();
        Task<List<WeeksView>> GetWeeksView();
        Task<bool> AddWeeks(Week weeks);
        Task<bool> UpdateWeeks(Week weeks);
        Task RemoveWeeks(int id);
        Task AddPlayersToArchive(List<Player> players);
        Task AddPlayersDataToArchive(List<PlayerDatum> playersData);
        Task AddTeamsToArchive(List<TeamDetail> teams);
        Task AddWeeksToArchive(List<Week> weeks);
        Task AddScheduleToArchive(List<Schedule> schedule);
        Task<List<Changelog>> GetChangelog();
    }
}
