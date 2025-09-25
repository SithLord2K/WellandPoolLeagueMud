using Microsoft.EntityFrameworkCore;
using WellandPoolLeagueMud.Data.Models;
using WellandPoolLeagueMud.Data.ViewModels;

namespace WellandPoolLeagueMud.Data.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly WellandPoolLeagueDbContext _context;

        public ScheduleService(WellandPoolLeagueDbContext context)
        {
            _context = context;
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
    }
}