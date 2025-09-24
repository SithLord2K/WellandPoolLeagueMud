using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WellandPoolLeagueMud.Data.Models;
using WellandPoolLeagueMud.Data.ViewModels;

namespace WellandPoolLeagueMud.Data.Services
{
    public class BarService : IBarService
    {
        private readonly WellandPoolLeagueDbContext _context;
        private readonly ILogger<BarService> _logger;

        public BarService(WellandPoolLeagueDbContext context, ILogger<BarService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<BarViewModel>> GetAllBarsAsync()
        {
            try
            {
                var bars = await _context.Bars
                    .Include(b => b.Teams)
                    .OrderBy(b => b.BarName)
                    .ToListAsync();

                return bars.Select(MapToViewModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all bars");
                throw;
            }
        }

        public async Task<List<BarViewModel>> GetActiveBarsAsync()
        {
            try
            {
                var bars = await _context.Bars
                    .Where(b => b.IsActive)
                    .Include(b => b.Teams)
                    .OrderBy(b => b.BarName)
                    .ToListAsync();

                return bars.Select(MapToViewModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active bars");
                throw;
            }
        }

        public async Task<BarViewModel?> GetBarByIdAsync(int barId)
        {
            try
            {
                var bar = await _context.Bars
                    .Include(b => b.Teams)
                    .FirstOrDefaultAsync(b => b.BarId == barId);

                return bar != null ? MapToViewModel(bar) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bar by ID: {BarId}", barId);
                throw;
            }
        }

        public async Task<BarViewModel?> GetBarByNameAsync(string barName)
        {
            try
            {
                var bar = await _context.Bars
                    .Include(b => b.Teams)
                    .FirstOrDefaultAsync(b => b.BarName.ToLower() == barName.ToLower());

                return bar != null ? MapToViewModel(bar) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bar by name: {BarName}", barName);
                throw;
            }
        }

        public async Task<int> CreateBarAsync(BarViewModel barViewModel)
        {
            try
            {
                var bar = new Bar
                {
                    BarName = barViewModel.BarName,
                    NumberOfTables = barViewModel.NumberOfTables,
                    Address = barViewModel.Address,
                    IsActive = barViewModel.IsActive,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };

                _context.Bars.Add(bar);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new bar: {BarName} with ID: {BarId}", bar.BarName, bar.BarId);
                return bar.BarId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bar: {BarName}", barViewModel.BarName);
                throw;
            }
        }

        public async Task<bool> UpdateBarAsync(BarViewModel barViewModel)
        {
            try
            {
                var bar = await _context.Bars.FindAsync(barViewModel.BarId);
                if (bar == null) return false;

                bar.BarName = barViewModel.BarName;
                bar.NumberOfTables = barViewModel.NumberOfTables;
                bar.Address = barViewModel.Address;
                bar.IsActive = barViewModel.IsActive;
                bar.ModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated bar: {BarName} with ID: {BarId}", bar.BarName, bar.BarId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bar with ID: {BarId}", barViewModel.BarId);
                throw;
            }
        }

        public async Task<bool> DeleteBarAsync(int barId)
        {
            try
            {
                var bar = await _context.Bars
                    .Include(b => b.Teams)
                    .FirstOrDefaultAsync(b => b.BarId == barId);

                if (bar == null) return false;

                // Check if bar has teams
                if (bar.Teams.Any())
                {
                    _logger.LogWarning("Cannot delete bar {BarId} - it has {TeamCount} teams assigned", barId, bar.Teams.Count);
                    return false;
                }

                _context.Bars.Remove(bar);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted bar: {BarName} with ID: {BarId}", bar.BarName, bar.BarId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting bar with ID: {BarId}", barId);
                throw;
            }
        }

        public async Task<bool> DeactivateBarAsync(int barId)
        {
            try
            {
                var bar = await _context.Bars.FindAsync(barId);
                if (bar == null) return false;

                bar.IsActive = false;
                bar.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deactivated bar: {BarName} with ID: {BarId}", bar.BarName, bar.BarId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating bar with ID: {BarId}", barId);
                throw;
            }
        }

        public async Task<bool> ActivateBarAsync(int barId)
        {
            try
            {
                var bar = await _context.Bars.FindAsync(barId);
                if (bar == null) return false;

                bar.IsActive = true;
                bar.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Activated bar: {BarName} with ID: {BarId}", bar.BarName, bar.BarId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating bar with ID: {BarId}", barId);
                throw;
            }
        }

        public async Task<List<TeamViewModel>> GetTeamsByBarIdAsync(int barId)
        {
            try
            {
                var teams = await _context.Teams
                    .Where(t => t.BarId == barId)
                    .Include(t => t.Bar)
                    .ToListAsync();

                return teams.Select(MapTeamToViewModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving teams for bar ID: {BarId}", barId);
                throw;
            }
        }

        public async Task<Dictionary<int, List<TeamViewModel>>> GetTeamsGroupedByBarAsync()
        {
            try
            {
                var teams = await _context.Teams
                    .Where(t => t.BarId.HasValue)
                    .Include(t => t.Bar)
                    .ToListAsync();

                return teams
                    .GroupBy(t => t.BarId!.Value)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(MapTeamToViewModel).ToList()
                    );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving teams grouped by bar");
                throw;
            }
        }

        public async Task<bool> AssignTeamToBarAsync(int teamId, int barId)
        {
            try
            {
                var team = await _context.Teams.FindAsync(teamId);
                var bar = await _context.Bars
                    .Include(b => b.Teams)
                    .FirstOrDefaultAsync(b => b.BarId == barId);

                if (team == null || bar == null) return false;

                // Check if bar has available slots
                if (bar.Teams.Count >= bar.NumberOfTables * 2)
                {
                    _logger.LogWarning("Cannot assign team {TeamId} to bar {BarId} - no available slots", teamId, barId);
                    return false;
                }

                team.BarId = barId;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Assigned team {TeamId} to bar {BarId}", teamId, barId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning team {TeamId} to bar {BarId}", teamId, barId);
                throw;
            }
        }

        public async Task<bool> RemoveTeamFromBarAsync(int teamId)
        {
            try
            {
                var team = await _context.Teams.FindAsync(teamId);
                if (team == null) return false;

                team.BarId = null;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Removed team {TeamId} from bar assignment", teamId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing team {TeamId} from bar", teamId);
                throw;
            }
        }

        public async Task<bool> CanBarAcceptMoreTeamsAsync(int barId)
        {
            try
            {
                var bar = await _context.Bars
                    .Include(b => b.Teams)
                    .FirstOrDefaultAsync(b => b.BarId == barId);

                if (bar == null) return false;
                return bar.Teams.Count < bar.NumberOfTables * 2;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if bar {BarId} can accept more teams", barId);
                throw;
            }
        }

        public async Task<int> GetAvailableSlotsForBarAsync(int barId)
        {
            try
            {
                var bar = await _context.Bars
                    .Include(b => b.Teams)
                    .FirstOrDefaultAsync(b => b.BarId == barId);

                if (bar == null) return 0;
                return Math.Max(0, (bar.NumberOfTables * 2) - bar.Teams.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available slots for bar {BarId}", barId);
                throw;
            }
        }

        public async Task<List<BarViewModel>> GetBarsWithAvailableSlotsAsync()
        {
            try
            {
                var bars = await _context.Bars
                    .Where(b => b.IsActive)
                    .Include(b => b.Teams)
                    .ToListAsync();

                return bars
                    .Where(b => b.Teams.Count < b.NumberOfTables * 2)
                    .Select(MapToViewModel)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bars with available slots");
                throw;
            }
        }

        private BarViewModel MapToViewModel(Bar bar)
        {
            return new BarViewModel
            {
                BarId = bar.BarId,
                BarName = bar.BarName,
                NumberOfTables = bar.NumberOfTables,
                Address = bar.Address,
                IsActive = bar.IsActive,
                CreatedDate = bar.CreatedDate,
                ModifiedDate = bar.ModifiedDate,
                Teams = bar.Teams?.Select(MapTeamToViewModel).ToList() ?? new List<TeamViewModel>()
            };
        }

        private TeamViewModel MapTeamToViewModel(Team team)
        {
            return new TeamViewModel
            {
                TeamId = team.TeamId,
                TeamName = team.TeamName,
                BarId = team.BarId,
                BarName = team.Bar?.BarName,
                BarTableCount = team.Bar?.NumberOfTables,
                // Note: You'll need to map other properties from your existing Team model
                // This is just showing the bar-related mappings
            };
        }
    }
}