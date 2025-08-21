using Microsoft.EntityFrameworkCore;
using WellandPoolLeagueMud.Data.Models;

namespace WellandPoolLeagueMud.Data.Services
{
    public class DataFactory(IDbContextFactory<WPLStatsDBContext> contextFactory)
    {
        /*public async Task<List<Player>> GetPlayers()
        {
            using var _context = contextFactory.CreateDbContext();
            return await _context.Players.ToListAsync();
        }
        public async Task<List<Changelog>> GetChangelog()
        {
            using var _context = contextFactory.CreateDbContext();
            return await _context.Changelog.ToListAsync();
        }*/

        private async Task<List<T>> GetListAsync<T>() where T : class
        {
            using var _context = contextFactory.CreateDbContext();
            return await _context.Set<T>().ToListAsync();
        }

        // Public methods now become one-liners
        public async Task<List<Player>> GetPlayers() => await GetListAsync<Player>();
        public async Task<List<Changelog>> GetChangelog() => await GetListAsync<Changelog>();
        public async Task<List<PlayerDatum>> GetPlayerData() => await GetListAsync<PlayerDatum>();
        public async Task<List<TeamDetail>> GetTeams() => await GetListAsync<TeamDetail>();

        public async Task<List<PlayersView>> GetPlayersView()
        {
            using var _context = contextFactory.CreateDbContext();
            return await _context.PlayersViews.ToListAsync();
        }
        public async Task<Player?> GetSinglePlayer(int id)
        {
            using var _context = contextFactory.CreateDbContext();
            return await _context.Players.FindAsync(id);
        }
        public async Task<List<PlayerDatum>> GetSinglePlayerData(int playerId)
        {
            using var _context = contextFactory.CreateDbContext();
            return await _context.PlayerData.Where(e => e.PlayerId == playerId).ToListAsync();
        }
        public Task<List<PlayerDatum>> GetAllPlayerData()
        {
            using var _context = contextFactory.CreateDbContext();
            return _context.PlayerData.ToListAsync();
        }
        public async Task<List<PlayerData>> GetPlayerData_All()
        {
            using var _context = contextFactory.CreateDbContext();

            // Perform a GroupJoin to get all player data and their associated teams in a single query.
            var query = from pd in _context.PlayerData
                        join p in _context.Players on pd.PlayerId equals p.Id into playerGroup
                        from player in playerGroup.DefaultIfEmpty()
                        select new PlayerData
                        {
                            PlayerId = pd.PlayerId,
                            GamesWon = pd.GamesWon,
                            GamesLost = pd.GamesLost,
                            WeekNumber = pd.WeekNumber,
                            GamesPlayed = pd.GamesPlayed,
                            Average = (pd.GamesPlayed != 0) ? (decimal)pd.GamesWon / pd.GamesPlayed : 0,
                            TeamId = player.TeamId
                        };

            return await query.ToListAsync();
        }
        public async Task<bool> AddPlayer(Player player)
        {
            using var _context = contextFactory.CreateDbContext();
            _context.Players.Add(player);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> SavePlayerData(PlayerDatum playerData)
        {
            using var _context = contextFactory.CreateDbContext();
            _context.PlayerData.Add(playerData);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Schedule>> GetSchedule()
        {
            using var _context = contextFactory.CreateDbContext();
            return await _context.Schedules.ToListAsync();
        }
        public async Task<Schedule> GetSingleSchedule(int Id)
        {
            using var _context = contextFactory.CreateDbContext();
            return await _context.Schedules.FindAsync(Id);
        }
        public async Task<bool> AddSchedule(Schedule schedule)
        {
            using var _context = contextFactory.CreateDbContext();
            var exists = await _context.Schedules.AnyAsync(e => e.Id == schedule.Id);
            if (exists)
            {
                _context.Schedules.Update(schedule);
                await _context.SaveChangesAsync();
                return true;
            }
            if (schedule.Date.ToString() == "0001-01-01" || schedule.Date.ToString() == null)
            {
                return false;
            }
            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<TeamDetail>> GetTeamDetails()
        {
            using var _context = contextFactory.CreateDbContext();
            return await _context.TeamDetails.ToListAsync();
        }
        public async Task<TeamDetail> GetSingleTeam(int Id)
        {
            using var _context = contextFactory.CreateDbContext();
            return await _context.TeamDetails.FindAsync(Id);
        }
        public async Task<bool> AddTeam(TeamDetail team)
        {
            using var _context = contextFactory.CreateDbContext();
            _context.TeamDetails.Add(team);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<Week>> GetAllWeeks()
        {
            using var _context = contextFactory.CreateDbContext();
            return await _context.Weeks.ToListAsync();
        }
        public async Task<List<WeeksView>> GetWeeksView()
        {
            using var _context = contextFactory.CreateDbContext();
            return await _context.WeeksViews.ToListAsync();
        }
        public async Task<bool> AddWeeks(Week weeks)
        {
            using var _context = contextFactory.CreateDbContext();
            _context.Weeks.Add(weeks);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdateWeeks(Week weeks)
        {
            using var _context = contextFactory.CreateDbContext();
            _context.Weeks.Update(weeks);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
