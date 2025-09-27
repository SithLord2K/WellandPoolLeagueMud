using Microsoft.EntityFrameworkCore;
using WellandPoolLeagueMud.Data.Models;
using WellandPoolLeagueMud.Data.ViewModels;
using Microsoft.Extensions.Logging;

namespace WellandPoolLeagueMud.Data.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly WellandPoolLeagueDbContext _context;
        private readonly ILogger<ScheduleService>? _logger;

        public ScheduleService(WellandPoolLeagueDbContext context, ILogger<ScheduleService>? logger = null)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ScheduleViewModel>> GetAllSchedulesAsync()
        {
            return await _context.Schedules
                .Include(s => s.HomeTeam)
                .Include(s => s.AwayTeam)
                .Include(s => s.WinningTeam)
                .Select(s => new ScheduleViewModel
                {
                    ScheduleId = s.ScheduleId,
                    WeekNumber = s.WeekNumber,
                    HomeTeamId = s.HomeTeamId,
                    HomeTeamName = s.HomeTeam.TeamName,
                    AwayTeamId = s.AwayTeamId,
                    AwayTeamName = s.AwayTeam.TeamName,
                    GameDate = s.GameDate,
                    WinningTeamId = s.WinningTeamId,
                    WinningTeamName = s.WinningTeam != null ? s.WinningTeam.TeamName : null,
                    IsComplete = s.IsComplete,
                    TableNumber = s.TableNumber
                })
                .OrderBy(s => s.WeekNumber)
                .ThenBy(s => s.GameDate)
                .ToListAsync();
        }

        public async Task<ScheduleViewModel?> GetScheduleByIdAsync(int id)
        {
            var schedule = await _context.Schedules
                .Include(s => s.HomeTeam)
                .Include(s => s.AwayTeam)
                .Include(s => s.WinningTeam)
                .FirstOrDefaultAsync(s => s.ScheduleId == id);

            if (schedule == null) return null;

            return new ScheduleViewModel
            {
                ScheduleId = schedule.ScheduleId,
                WeekNumber = schedule.WeekNumber,
                HomeTeamId = schedule.HomeTeamId,
                HomeTeamName = schedule.HomeTeam.TeamName,
                AwayTeamId = schedule.AwayTeamId,
                AwayTeamName = schedule.AwayTeam.TeamName,
                GameDate = schedule.GameDate,
                WinningTeamId = schedule.WinningTeamId,
                WinningTeamName = schedule.WinningTeam?.TeamName,
                IsComplete = schedule.IsComplete,
                TableNumber = schedule.TableNumber
            };
        }

        public async Task<ScheduleViewModel> CreateScheduleAsync(ScheduleViewModel scheduleVM)
        {
            var homeTeam = await _context.Teams.FindAsync(scheduleVM.HomeTeamId);
            var awayTeam = await _context.Teams.FindAsync(scheduleVM.AwayTeamId);

            var homeTeamAlreadyScheduled = await _context.Schedules
                .AnyAsync(s => s.WeekNumber == scheduleVM.WeekNumber && (s.HomeTeamId == scheduleVM.HomeTeamId || s.AwayTeamId == scheduleVM.HomeTeamId));

            if (homeTeamAlreadyScheduled)
            {
                throw new InvalidOperationException($"{homeTeam?.TeamName} is already scheduled to play in week {scheduleVM.WeekNumber}.");
            }

            var awayTeamAlreadyScheduled = await _context.Schedules
                .AnyAsync(s => s.WeekNumber == scheduleVM.WeekNumber && (s.HomeTeamId == scheduleVM.AwayTeamId || s.AwayTeamId == scheduleVM.AwayTeamId));

            if (awayTeamAlreadyScheduled)
            {
                throw new InvalidOperationException($"{awayTeam?.TeamName} is already scheduled to play in week {scheduleVM.WeekNumber}.");
            }

            var schedule = new Schedule
            {
                WeekNumber = scheduleVM.WeekNumber,
                HomeTeamId = scheduleVM.HomeTeamId,
                AwayTeamId = scheduleVM.AwayTeamId,
                GameDate = (DateTime)scheduleVM.GameDate!,
                WinningTeamId = scheduleVM.WinningTeamId,
                IsComplete = scheduleVM.IsComplete,
                TableNumber = scheduleVM.TableNumber
            };

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            scheduleVM.ScheduleId = schedule.ScheduleId;
            return scheduleVM;
        }

        /// <summary>
        /// Efficiently saves a list of schedules to the database in a single transaction
        /// </summary>
        public async Task<List<ScheduleViewModel>> CreateScheduleBatchAsync(List<ScheduleViewModel> schedules)
        {
            if (schedules == null || !schedules.Any())
                return new List<ScheduleViewModel>();

            // Clear any existing tracked entities to avoid conflicts
            _context.ChangeTracker.Clear();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Validate all schedules before creating any
                await ValidateScheduleBatchAsync(schedules);

                // Create completely new Schedule entities with no reference to existing tracking
                var scheduleEntities = new List<Schedule>();

                for (int i = 0; i < schedules.Count; i++)
                {
                    var vm = schedules[i];

                    // Ensure ScheduleId is 0 for new entities
                    vm.ScheduleId = 0;

                    var schedule = new Schedule
                    {
                        // Explicitly do NOT set ScheduleId - let EF auto-generate
                        WeekNumber = vm.WeekNumber,
                        HomeTeamId = vm.HomeTeamId,
                        AwayTeamId = vm.AwayTeamId,
                        GameDate = vm.GameDate ?? DateTime.Now,
                        WinningTeamId = vm.WinningTeamId,
                        IsComplete = vm.IsComplete,
                        TableNumber = vm.TableNumber
                    };

                    scheduleEntities.Add(schedule);
                }

                // Add all schedules to context
                _context.Schedules.AddRange(scheduleEntities);

                // Save all changes in one go
                var savedCount = await _context.SaveChangesAsync();

                // Update the ViewModels with generated IDs
                for (int i = 0; i < schedules.Count; i++)
                {
                    schedules[i].ScheduleId = scheduleEntities[i].ScheduleId;
                }

                await transaction.CommitAsync();

                _logger?.LogInformation("Successfully saved {Count} schedules in batch", savedCount);
                return schedules;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger?.LogError(ex, "Error in CreateScheduleBatchAsync for {Count} schedules", schedules.Count);
                throw;
            }
            finally
            {
                // Clear the change tracker after the operation
                _context.ChangeTracker.Clear();
            }
        }

        /// <summary>
        /// Validates a batch of schedules for conflicts without saving them
        /// </summary>
        private async Task ValidateScheduleBatchAsync(List<ScheduleViewModel> schedules)
        {
            // Get all existing schedules for validation
            var allWeeks = schedules.Select(s => s.WeekNumber).Distinct().ToList();

            // Create a simple projection to avoid complex anonymous types
            var existingSchedules = new List<ScheduleConflictCheck>();

            var existingData = await _context.Schedules
                .AsNoTracking()
                .Where(s => allWeeks.Contains(s.WeekNumber))
                .ToListAsync();

            existingSchedules = existingData.Select(s => new ScheduleConflictCheck
            {
                WeekNumber = s.WeekNumber,
                HomeTeamId = s.HomeTeamId,
                AwayTeamId = s.AwayTeamId
            }).ToList();

            // Validate each schedule
            foreach (var schedule in schedules)
            {
                // Ensure ScheduleId is 0 for new entities
                schedule.ScheduleId = 0;

                // Check against existing schedules in database
                var homeTeamConflict = existingSchedules.Any(s =>
                    s.WeekNumber == schedule.WeekNumber &&
                    (s.HomeTeamId == schedule.HomeTeamId || s.AwayTeamId == schedule.HomeTeamId));

                if (homeTeamConflict)
                {
                    var homeTeam = await _context.Teams
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.TeamId == schedule.HomeTeamId);
                    throw new InvalidOperationException($"{homeTeam?.TeamName} is already scheduled to play in week {schedule.WeekNumber}.");
                }

                var awayTeamConflict = existingSchedules.Any(s =>
                    s.WeekNumber == schedule.WeekNumber &&
                    (s.HomeTeamId == schedule.AwayTeamId || s.AwayTeamId == schedule.AwayTeamId));

                if (awayTeamConflict)
                {
                    var awayTeam = await _context.Teams
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.TeamId == schedule.AwayTeamId);
                    throw new InvalidOperationException($"{awayTeam?.TeamName} is already scheduled to play in week {schedule.WeekNumber}.");
                }

                // Check against other schedules in the same batch
                var batchConflicts = schedules.Where(s => s != schedule && s.WeekNumber == schedule.WeekNumber)
                    .Any(s => s.HomeTeamId == schedule.HomeTeamId || s.AwayTeamId == schedule.HomeTeamId ||
                             s.HomeTeamId == schedule.AwayTeamId || s.AwayTeamId == schedule.AwayTeamId);

                if (batchConflicts)
                {
                    throw new InvalidOperationException($"Team conflict found in batch for week {schedule.WeekNumber}.");
                }
            }
        }

        public async Task<ScheduleViewModel?> UpdateScheduleAsync(ScheduleViewModel scheduleVM)
        {
            var schedule = await _context.Schedules.FindAsync(scheduleVM.ScheduleId);
            if (schedule == null) return null;

            var homeTeam = await _context.Teams.FindAsync(scheduleVM.HomeTeamId);
            var awayTeam = await _context.Teams.FindAsync(scheduleVM.AwayTeamId);

            var homeTeamAlreadyScheduled = await _context.Schedules
                .AnyAsync(s => s.ScheduleId != scheduleVM.ScheduleId && s.WeekNumber == scheduleVM.WeekNumber && (s.HomeTeamId == scheduleVM.HomeTeamId || s.AwayTeamId == scheduleVM.HomeTeamId));

            if (homeTeamAlreadyScheduled)
            {
                throw new InvalidOperationException($"{homeTeam?.TeamName} is already scheduled to play in week {scheduleVM.WeekNumber} in another game.");
            }

            var awayTeamAlreadyScheduled = await _context.Schedules
                .AnyAsync(s => s.ScheduleId != scheduleVM.ScheduleId && s.WeekNumber == scheduleVM.WeekNumber && (s.HomeTeamId == scheduleVM.AwayTeamId || s.AwayTeamId == scheduleVM.AwayTeamId));

            if (awayTeamAlreadyScheduled)
            {
                throw new InvalidOperationException($"{awayTeam?.TeamName} is already scheduled to play in week {scheduleVM.WeekNumber} in another game.");
            }

            schedule.WeekNumber = scheduleVM.WeekNumber;
            schedule.HomeTeamId = scheduleVM.HomeTeamId;
            schedule.AwayTeamId = scheduleVM.AwayTeamId;
            schedule.GameDate = (DateTime)scheduleVM.GameDate!;
            schedule.WinningTeamId = scheduleVM.WinningTeamId;
            schedule.IsComplete = scheduleVM.IsComplete;
            schedule.TableNumber = scheduleVM.TableNumber;

            await _context.SaveChangesAsync();
            return scheduleVM;
        }

        public async Task<bool> DeleteScheduleAsync(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null) return false;

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ScheduleViewModel>> GetSchedulesByWeekAsync(int weekNumber)
        {
            return await _context.Schedules
                .Include(s => s.HomeTeam)
                .Include(s => s.AwayTeam)
                .Include(s => s.WinningTeam)
                .Where(s => s.WeekNumber == weekNumber)
                .Select(s => new ScheduleViewModel
                {
                    ScheduleId = s.ScheduleId,
                    WeekNumber = s.WeekNumber,
                    HomeTeamId = s.HomeTeamId,
                    HomeTeamName = s.HomeTeam.TeamName,
                    AwayTeamId = s.AwayTeamId,
                    AwayTeamName = s.AwayTeam.TeamName,
                    GameDate = s.GameDate,
                    WinningTeamId = s.WinningTeamId,
                    WinningTeamName = s.WinningTeam != null ? s.WinningTeam.TeamName : null,
                    IsComplete = s.IsComplete,
                    TableNumber = s.TableNumber
                })
                .OrderBy(s => s.GameDate)
                .ToListAsync();
        }

        public async Task<List<ScheduleViewModel>> GetSchedulesByTeamAsync(int teamId)
        {
            return await _context.Schedules
                .Include(s => s.HomeTeam)
                .Include(s => s.AwayTeam)
                .Include(s => s.WinningTeam)
                .Where(s => s.HomeTeamId == teamId || s.AwayTeamId == teamId)
                .Select(s => new ScheduleViewModel
                {
                    ScheduleId = s.ScheduleId,
                    WeekNumber = s.WeekNumber,
                    HomeTeamId = s.HomeTeamId,
                    HomeTeamName = s.HomeTeam.TeamName,
                    AwayTeamId = s.AwayTeamId,
                    AwayTeamName = s.AwayTeam.TeamName,
                    GameDate = s.GameDate,
                    WinningTeamId = s.WinningTeamId,
                    WinningTeamName = s.WinningTeam != null ? s.WinningTeam.TeamName : null,
                    IsComplete = s.IsComplete,
                    TableNumber = s.TableNumber
                })
                .OrderBy(s => s.WeekNumber)
                .ThenBy(s => s.GameDate)
                .ToListAsync();
        }

        public async Task<List<ScheduleViewModel>> GetCompletedSchedulesAsync()
        {
            return await _context.Schedules
                .Include(s => s.HomeTeam)
                .Include(s => s.AwayTeam)
                .Include(s => s.WinningTeam)
                .Where(s => s.IsComplete)
                .Select(s => new ScheduleViewModel
                {
                    ScheduleId = s.ScheduleId,
                    WeekNumber = s.WeekNumber,
                    HomeTeamId = s.HomeTeamId,
                    HomeTeamName = s.HomeTeam.TeamName,
                    AwayTeamId = s.AwayTeamId,
                    AwayTeamName = s.AwayTeam.TeamName,
                    GameDate = s.GameDate,
                    WinningTeamId = s.WinningTeamId,
                    WinningTeamName = s.WinningTeam!.TeamName,
                    IsComplete = s.IsComplete,
                    TableNumber = s.TableNumber
                })
                .OrderByDescending(s => s.GameDate)
                .ToListAsync();
        }

        public async Task<List<ScheduleViewModel>> GetUpcomingSchedulesAsync()
        {
            return await _context.Schedules
                .Include(s => s.HomeTeam)
                .Include(s => s.AwayTeam)
                .Include(s => s.WinningTeam)
                .Where(s => !s.IsComplete)
                .Select(s => new ScheduleViewModel
                {
                    ScheduleId = s.ScheduleId,
                    WeekNumber = s.WeekNumber,
                    HomeTeamId = s.HomeTeamId,
                    HomeTeamName = s.HomeTeam.TeamName,
                    AwayTeamId = s.AwayTeamId,
                    AwayTeamName = s.AwayTeam.TeamName,
                    GameDate = s.GameDate,
                    WinningTeamId = s.WinningTeamId,
                    WinningTeamName = s.WinningTeam!.TeamName,
                    IsComplete = s.IsComplete,
                    TableNumber = s.TableNumber
                })
                .OrderBy(s => s.GameDate)
                .ToListAsync();
        }

        public async Task<bool> ScheduleExistsAsync(int id)
        {
            return await _context.Schedules.AnyAsync(s => s.ScheduleId == id);
        }

        public async Task<bool> CompleteScheduleAsync(int scheduleId, int winningTeamId)
        {
            var schedule = await _context.Schedules.FindAsync(scheduleId);
            if (schedule == null) return false;

            if (schedule.HomeTeamId != winningTeamId && schedule.AwayTeamId != winningTeamId)
                return false;

            schedule.WinningTeamId = winningTeamId;
            schedule.IsComplete = true;

            await _context.SaveChangesAsync();
            return true;
        }

        // Helper class for validation
        private class ScheduleConflictCheck
        {
            public int WeekNumber { get; set; }
            public int HomeTeamId { get; set; }
            public int AwayTeamId { get; set; }
        }
    }
}