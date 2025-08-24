// Data/Services/DataFactory.cs
using Microsoft.EntityFrameworkCore;
using WellandPoolLeagueMud.Data.Models;
using WellandPoolLeagueMud.Data;
using System.Reflection; // Required for the generic Save/Delete methods
using Microsoft.Extensions.Logging; // Required for logging

namespace WellandPoolLeagueMud.Data.Services
{
    public class DataFactory : IDataFactory
    {
        private readonly IDbContextFactory<WPLMudDBContext> contextFactory;
        private readonly ILogger<DataFactory> _logger;

        public DataFactory(IDbContextFactory<WPLMudDBContext> contextFactory, ILogger<DataFactory> logger)
        {
            this.contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<WPL_Player>> GetPlayersAsync()
        {
            using var context = contextFactory.CreateDbContext();
            return await context.Players.ToListAsync();
        }

        public async Task<WPL_Player?> GetSinglePlayerAsync(int id)
        {
            using var context = contextFactory.CreateDbContext();
            return await context.Players.FindAsync(id);
        }

        public async Task<bool> SavePlayerAsync(WPL_Player player)
        {
            using var context = contextFactory.CreateDbContext();
            if (player.PlayerId == 0)
                context.Players.Add(player);
            else
                context.Players.Update(player);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeletePlayerAsync(int id)
        {
            using var context = contextFactory.CreateDbContext();
            var player = await context.Players.FindAsync(id);
            if (player == null) return false;
            context.Players.Remove(player);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<List<WPL_Team>> GetTeamsAsync()
        {
            using var context = contextFactory.CreateDbContext();
            return await context.Teams.ToListAsync();
        }

        public async Task<WPL_Team?> GetSingleTeamAsync(int id)
        {
            using var context = contextFactory.CreateDbContext();
            return await context.Teams.FindAsync(id);
        }

        public async Task<bool> SaveTeamAsync(WPL_Team team)
        {
            using var context = contextFactory.CreateDbContext();
            if (team.TeamId == 0)
                context.Teams.Add(team);
            else
                context.Teams.Update(team);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteTeamAsync(int id)
        {
            using var context = contextFactory.CreateDbContext();
            var team = await context.Teams.FindAsync(id);
            if (team == null) return false;
            context.Teams.Remove(team);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<List<WPL_Schedule>> GetSchedulesAsync()
        {
            using var context = contextFactory.CreateDbContext();
            return await context.Schedules.ToListAsync();
        }

        public async Task<WPL_Schedule?> GetSingleScheduleAsync(int id)
        {
            using var context = contextFactory.CreateDbContext();
            return await context.Schedules.FindAsync(id);
        }

        public async Task<bool> AddOrUpdateScheduleAsync(WPL_Schedule schedule)
        {
            using var context = contextFactory.CreateDbContext();
            if (schedule.ScheduleId == 0)
                context.Schedules.Add(schedule);
            else
                context.Schedules.Update(schedule);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteScheduleAsync(int id)
        {
            using var context = contextFactory.CreateDbContext();
            var schedule = await context.Schedules.FindAsync(id);
            if (schedule == null) return false;
            context.Schedules.Remove(schedule);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<List<WPL_PlayerGame>> GetAllPlayerGamesAsync()
        {
            using var context = contextFactory.CreateDbContext();
            return await context.PlayerGames.ToListAsync();
        }

        public async Task<List<WPL_PlayerGame>> GetPlayerGamesByPlayerIdAsync(int playerId)
        {
            using var context = contextFactory.CreateDbContext();
            return await context.PlayerGames.Where(pg => pg.PlayerId == playerId).ToListAsync();
        }

        public async Task<bool> SavePlayerGameAsync(WPL_PlayerGame playerGame)
        {
            using var context = contextFactory.CreateDbContext();
            if (playerGame.PlayerGameId == 0)
                context.PlayerGames.Add(playerGame);
            else
                context.PlayerGames.Update(playerGame);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeletePlayerGameAsync(int id)
        {
            using var context = contextFactory.CreateDbContext();
            var playerGame = await context.PlayerGames.FindAsync(id);
            if (playerGame == null) return false;
            context.PlayerGames.Remove(playerGame);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<List<WPL_WeeklyWinner>> GetWeeklyWinnersAsync()
        {
            using var context = contextFactory.CreateDbContext();
            return await context.WeeklyWinners.ToListAsync();
        }

        public async Task<WPL_WeeklyWinner?> GetSingleWeeklyWinnerAsync(int id)
        {
            using var context = contextFactory.CreateDbContext();
            return await context.WeeklyWinners.FindAsync(id);
        }

        public async Task<bool> AddOrUpdateWeeklyWinnerAsync(WPL_WeeklyWinner weeklyWinner)
        {
            using var context = contextFactory.CreateDbContext();
            if (weeklyWinner.WeeklyWinnerId == 0)
                context.WeeklyWinners.Add(weeklyWinner);
            else
                context.WeeklyWinners.Update(weeklyWinner);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteWeeklyWinnerAsync(int id)
        {
            using var context = contextFactory.CreateDbContext();
            var weeklyWinner = await context.WeeklyWinners.FindAsync(id);
            if (weeklyWinner == null) return false;
            context.WeeklyWinners.Remove(weeklyWinner);
            return await context.SaveChangesAsync() > 0;
        }

    }
}