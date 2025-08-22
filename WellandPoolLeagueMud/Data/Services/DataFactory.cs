using Microsoft.EntityFrameworkCore;
using WellandPoolLeagueMud.Data.Models;

namespace WellandPoolLeagueMud.Data.Services
{
    public class DataFactory(IDbContextFactory<WPLStatsDBContext> contextFactory)
    {
        private async Task<List<T>> GetListAsync<T>() where T : class
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            return await context.Set<T>().ToListAsync();
        }

        private async Task<T?> GetSingleAsync<T>(int id) where T : class
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            return await context.Set<T>().FindAsync(id);
        }

        public async Task<List<Player>> GetPlayers() => await GetListAsync<Player>();

        public async Task<List<PlayerDatum>> GetPlayerDatumAsync() => await GetListAsync<PlayerDatum>();

        public async Task<Player?> GetSinglePlayer(int id) => await GetSingleAsync<Player>(id);

        public async Task<List<PlayerDatum>> GetSinglePlayerData(int playerId)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            return await context.PlayerData.Where(e => e.PlayerId == playerId).ToListAsync();
        }

        public async Task<List<TeamDetail>> GetTeamDetails() => await GetListAsync<TeamDetail>();

        public async Task<TeamDetail?> GetSingleTeam(int id) => await GetSingleAsync<TeamDetail>(id);

        public async Task<List<Schedule>> GetSchedules() => await GetListAsync<Schedule>();

        public async Task<Schedule?> GetSingleSchedule(int id) => await GetSingleAsync<Schedule>(id);

        public async Task<List<Week>> GetAllWeeks() => await GetListAsync<Week>();

        public async Task<List<WeeksView>> GetWeeksView() => await GetListAsync<WeeksView>();

        public async Task<List<PlayersView>> GetPlayersView() => await GetListAsync<PlayersView>();

        public async Task<List<PlayerData>> GetAllPlayerDataAsync()
        {
            await using var context = await contextFactory.CreateDbContextAsync();

            var query = from pd in context.PlayerData
                        join p in context.Players on pd.PlayerId equals p.Id
                        select new PlayerData
                        {
                            PlayerId = pd.PlayerId,
                            GamesWon = pd.GamesWon,
                            GamesLost = pd.GamesLost,
                            WeekNumber = pd.WeekNumber,
                            GamesPlayed = pd.GamesPlayed,
                            Average = (pd.GamesPlayed != 0) ? (decimal)pd.GamesWon / pd.GamesPlayed : 0,
                            TeamId = p.TeamId
                        };

            return await query.ToListAsync();
        }

        public async Task<bool> AddPlayer(Player player)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            context.Players.Add(player);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddOrUpdateSchedule(Schedule schedule)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            context.Schedules.Update(schedule);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddTeam(TeamDetail team)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            context.TeamDetails.Add(team);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddWeeks(Week weeks)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            context.Weeks.Add(weeks);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateWeeks(Week weeks)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            context.Weeks.Update(weeks);
            await context.SaveChangesAsync();
            return true;
        }
    }
}