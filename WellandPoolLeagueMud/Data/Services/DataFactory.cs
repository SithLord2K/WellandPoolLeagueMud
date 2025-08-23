// Data/Services/DataFactory.cs
using Microsoft.EntityFrameworkCore;
using WellandPoolLeagueMud.Data.Models;
using WellandPoolLeagueMud.Data;
using System.Reflection; // Required for the generic Save/Delete methods

namespace WellandPoolLeagueMud.Data.Services
{
    public class DataFactory(IDbContextFactory<WPLMudDBContext> contextFactory) : IDataFactory
    {
        // === GENERIC METHODS FOR REUSABILITY ===

        // Gets a list of entities of a specific type.
        private async Task<List<T>> GetListAsync<T>() where T : class =>
            await (await contextFactory.CreateDbContextAsync()).Set<T>().ToListAsync();

        // Gets a single entity by its ID. Assumes the entity has a primary key named "Id".
        private async Task<T?> GetSingleAsync<T>(int id) where T : class =>
            await (await contextFactory.CreateDbContextAsync()).Set<T>().FindAsync(id);

        // A generic save method that handles both adding and updating.
        private async Task<bool> SaveEntityAsync<T>(T entity, string idPropertyName) where T : class
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            try
            {
                var idProperty = typeof(T).GetProperty(idPropertyName);
                var idValue = (int?)idProperty?.GetValue(entity);

                if (idValue is null || idValue == 0)
                {
                    context.Entry(entity).State = EntityState.Added;
                }
                else
                {
                    context.Entry(entity).State = EntityState.Modified;
                }
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // A generic method to delete an entity by ID.
        private async Task<bool> DeleteEntityAsync<T>(int id) where T : class
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            try
            {
                var entity = await context.Set<T>().FindAsync(id);
                if (entity == null) return false;

                context.Set<T>().Remove(entity);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // === IMPLEMENTATION OF IDataFactory ===

        // Players
        public async Task<List<WPL_Player>> GetPlayersAsync() => await GetListAsync<WPL_Player>();
        public async Task<WPL_Player?> GetSinglePlayerAsync(int id) => await GetSingleAsync<WPL_Player>(id);
        public async Task<bool> SavePlayerAsync(WPL_Player player) => await SaveEntityAsync(player, "PlayerId");
        public async Task<bool> DeletePlayerAsync(int id) => await DeleteEntityAsync<WPL_Player>(id);

        // Teams
        public async Task<List<WPL_Team>> GetTeamsAsync() => await GetListAsync<WPL_Team>();
        public async Task<WPL_Team?> GetSingleTeamAsync(int Id) => await GetSingleAsync<WPL_Team>(Id);
        public async Task<bool> SaveTeamAsync(WPL_Team team) => await SaveEntityAsync(team, "TeamId");
        public async Task<bool> DeleteTeamAsync(int id) => await DeleteEntityAsync<WPL_Team>(id);

        // Schedules
        public async Task<List<WPL_Schedule>> GetSchedulesAsync() => await GetListAsync<WPL_Schedule>();
        public async Task<WPL_Schedule?> GetSingleScheduleAsync(int Id) => await GetSingleAsync<WPL_Schedule>(Id);
        public async Task<bool> AddOrUpdateScheduleAsync(WPL_Schedule schedule) => await SaveEntityAsync(schedule, "ScheduleId");
        public async Task<bool> DeleteScheduleAsync(int id) => await DeleteEntityAsync<WPL_Schedule>(id);

        // Player Games
        public async Task<List<WPL_PlayerGame>> GetAllPlayerGamesAsync() => await GetListAsync<WPL_PlayerGame>();
        public async Task<List<WPL_PlayerGame>> GetPlayerGamesByPlayerIdAsync(int playerId)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            return await context.PlayerGames.Where(pg => pg.PlayerId == playerId).ToListAsync();
        }
        public async Task<bool> SavePlayerGameAsync(WPL_PlayerGame playerGame) => await SaveEntityAsync(playerGame, "PlayerGameId");
        public async Task<bool> DeletePlayerGameAsync(int id) => await DeleteEntityAsync<WPL_PlayerGame>(id);

        // Weekly Winners
        public async Task<List<WPL_WeeklyWinner>> GetWeeklyWinnersAsync() => await GetListAsync<WPL_WeeklyWinner>();
        public async Task<WPL_WeeklyWinner?> GetSingleWeeklyWinnerAsync(int id) => await GetSingleAsync<WPL_WeeklyWinner>(id);
        public async Task<bool> AddOrUpdateWeeklyWinnerAsync(WPL_WeeklyWinner weeklyWinner) => await SaveEntityAsync(weeklyWinner, "WeeklyWinnerId");
        public async Task<bool> DeleteWeeklyWinnerAsync(int id) => await DeleteEntityAsync<WPL_WeeklyWinner>(id);
    }
}