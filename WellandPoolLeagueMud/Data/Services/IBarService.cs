using WellandPoolLeagueMud.Data.ViewModels;

namespace WellandPoolLeagueMud.Data.Services
{
    public interface IBarService
    {
        // Basic CRUD operations
        Task<List<BarViewModel>> GetAllBarsAsync();
        Task<List<BarViewModel>> GetActiveBarsAsync();
        Task<BarViewModel?> GetBarByIdAsync(int barId);
        Task<BarViewModel?> GetBarByNameAsync(string barName);
        Task<int> CreateBarAsync(BarViewModel bar);
        Task<bool> UpdateBarAsync(BarViewModel bar);
        Task<bool> DeleteBarAsync(int barId);
        Task<bool> DeactivateBarAsync(int barId);
        Task<bool> ActivateBarAsync(int barId);

        // Team relationship operations
        Task<List<TeamViewModel>> GetTeamsByBarIdAsync(int barId);
        Task<Dictionary<int, List<TeamViewModel>>> GetTeamsGroupedByBarAsync();
        Task<bool> AssignTeamToBarAsync(int teamId, int barId);
        Task<bool> RemoveTeamFromBarAsync(int teamId);

        // Validation operations
        Task<bool> CanBarAcceptMoreTeamsAsync(int barId);
        Task<int> GetAvailableSlotsForBarAsync(int barId);
        Task<List<BarViewModel>> GetBarsWithAvailableSlotsAsync();
    }
}