using Microsoft.EntityFrameworkCore;
using WellandPoolLeagueMud.Data.Models;
using WellandPoolLeagueMud.Data.ViewModels;

namespace WellandPoolLeagueMud.Data.Services
{
    public class TeamService : ITeamService
    {
        private readonly WellandPoolLeagueDbContext _context;

        public TeamService(WellandPoolLeagueDbContext context)
        {
            _context = context;
        }

        public async Task<List<TeamViewModel>> GetAllTeamsAsync()
        {
            return await _context.Teams
                .Include(t => t.Captain)
                .Include(t => t.HomeGames)
                .Include(t => t.AwayGames)
                .Include(t => t.WonGames)
                .Select(t => new TeamViewModel
                {
                    TeamId = t.TeamId,
                    TeamName = t.TeamName,
                    CaptainPlayerId = t.CaptainPlayerId,
                    CaptainName = t.Captain != null ? (string.IsNullOrEmpty(t.Captain.LastName) ? t.Captain.FirstName : $"{t.Captain.FirstName} {t.Captain.LastName}") : null,
                    GamesPlayed = t.HomeGames.Count + t.AwayGames.Count,
                    GamesWon = t.WonGames.Count,
                    GamesLost = (t.HomeGames.Count + t.AwayGames.Count) - t.WonGames.Count
                })
                .OrderBy(t => t.TeamName)
                .ToListAsync();
        }

        public async Task<TeamViewModel?> GetTeamByIdAsync(int id)
        {
            var team = await _context.Teams
                .Include(t => t.Captain)
                .Include(t => t.HomeGames)
                .Include(t => t.AwayGames)
                .Include(t => t.WonGames)
                .FirstOrDefaultAsync(t => t.TeamId == id);

            if (team == null) return null;

            return new TeamViewModel
            {
                TeamId = team.TeamId,
                TeamName = team.TeamName,
                CaptainPlayerId = team.CaptainPlayerId,
                CaptainName = team.Captain != null ? (string.IsNullOrEmpty(team.Captain.LastName) ? team.Captain.FirstName : $"{team.Captain.FirstName} {team.Captain.LastName}") : null,
                GamesPlayed = team.HomeGames.Count + team.AwayGames.Count,
                GamesWon = team.WonGames.Count,
                GamesLost = (team.HomeGames.Count + team.AwayGames.Count) - team.WonGames.Count
            };
        }

        public async Task<TeamViewModel> CreateTeamAsync(TeamViewModel teamVM)
        {
            var team = new Team
            {
                TeamName = teamVM.TeamName,
                CaptainPlayerId = teamVM.CaptainPlayerId
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            teamVM.TeamId = team.TeamId;
            return teamVM;
        }

        public async Task<TeamViewModel?> UpdateTeamAsync(TeamViewModel teamVM)
        {
            var team = await _context.Teams.FindAsync(teamVM.TeamId);
            if (team == null) return null;

            team.TeamName = teamVM.TeamName;
            team.CaptainPlayerId = teamVM.CaptainPlayerId;

            await _context.SaveChangesAsync();
            return teamVM;
        }

        public async Task<bool> DeleteTeamAsync(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null) return false;

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<TeamStandingViewModel>> GetTeamStandingsAsync()
        {
            var standings = await _context.Teams
                .Include(t => t.Captain)
                .Include(t => t.HomeGames.Where(g => g.IsComplete))
                .Include(t => t.AwayGames.Where(g => g.IsComplete))
                .Include(t => t.WonGames)
                .Select(t => new TeamStandingViewModel
                {
                    TeamId = t.TeamId,
                    TeamName = t.TeamName,
                    CaptainName = t.Captain != null ? (string.IsNullOrEmpty(t.Captain.LastName) ? t.Captain.FirstName : $"{t.Captain.FirstName} {t.Captain.LastName}") : null,
                    GamesPlayed = t.HomeGames.Count(g => g.IsComplete) + t.AwayGames.Count(g => g.IsComplete),
                    Wins = t.WonGames.Count,
                    Losses = (t.HomeGames.Count(g => g.IsComplete) + t.AwayGames.Count(g => g.IsComplete)) - t.WonGames.Count,
                    Points = t.WonGames.Count * 2,
                    WinPercentage = (t.HomeGames.Count(g => g.IsComplete) + t.AwayGames.Count(g => g.IsComplete)) > 0 ?
                        (decimal)t.WonGames.Count / (t.HomeGames.Count(g => g.IsComplete) + t.AwayGames.Count(g => g.IsComplete)) * 100 : 0
                })
                .OrderByDescending(ts => ts.Points)
                .ThenByDescending(ts => ts.WinPercentage)
                .ToListAsync();

            // Add ranks
            for (int i = 0; i < standings.Count; i++)
            {
                standings[i].Rank = i + 1;
            }

            return standings;
        }

        public async Task<List<PlayerViewModel>> GetTeamRosterAsync(int teamId)
        {
            return await _context.PlayerGames
                .Where(pg => pg.TeamId == teamId)
                .Select(pg => pg.Player)
                .Distinct()
                .Select(p => new PlayerViewModel
                {
                    PlayerId = p.PlayerId,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Phone = p.Phone,
                    GamesPlayed = p.PlayerGames.Count(pg => pg.TeamId == teamId),
                    GamesWon = p.PlayerGames.Count(pg => pg.TeamId == teamId && pg.IsWin),
                    GamesLost = p.PlayerGames.Count(pg => pg.TeamId == teamId && !pg.IsWin)
                })
                .OrderBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .ToListAsync();
        }

        public async Task<bool> TeamExistsAsync(int id)
        {
            return await _context.Teams.AnyAsync(t => t.TeamId == id);
        }
    }
}