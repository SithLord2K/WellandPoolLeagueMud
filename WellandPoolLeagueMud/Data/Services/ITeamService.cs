using WellandPoolLeagueMud.Data.ViewModels;

namespace WellandPoolLeagueMud.Data.Services
{
    public interface ITeamService
    {
        Task<List<TeamViewModel>> GetAllTeamsAsync();
        Task<TeamViewModel?> GetTeamByIdAsync(int id);
        Task<TeamViewModel> CreateTeamAsync(TeamViewModel teamVM);
        Task<TeamViewModel?> UpdateTeamAsync(TeamViewModel teamVM);
        Task<bool> DeleteTeamAsync(int id);
        Task<List<TeamStandingViewModel>> GetTeamStandingsAsync();
        Task<List<PlayerViewModel>> GetTeamRosterAsync(int teamId);
        Task<bool> TeamExistsAsync(int id);
    }
}