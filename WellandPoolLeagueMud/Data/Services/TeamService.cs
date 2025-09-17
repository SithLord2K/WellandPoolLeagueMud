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
            var teams = await _context.Teams
                .Include(t => t.Captain)
                .Include(t => t.PlayerGames)
                .Select(t => new TeamViewModel
                {
                    TeamId = t.TeamId,
                    TeamName = t.TeamName,
                    CaptainPlayerId = t.CaptainPlayerId,
                    CaptainName = t.Captain != null ? (string.IsNullOrEmpty(t.Captain.LastName) ? t.Captain.FirstName : $"{t.Captain.FirstName} {t.Captain.LastName}") : null,
                    GamesPlayed = t.PlayerGames.Sum(pg => pg.Wins + pg.Losses),
                    GamesWon = t.PlayerGames.Sum(pg => pg.Wins),
                    GamesLost = t.PlayerGames.Sum(pg => pg.Losses)
                })
                .OrderBy(t => t.TeamName)
                .ToListAsync();

            var allSchedules = await _context.Schedules.Where(s => s.WinningTeamId != null).ToListAsync();

            foreach (var team in teams)
            {
                // CORRECTED LOGIC: Use WinningTeamId to determine weekly record
                team.WeeksWon = allSchedules.Count(s => s.WinningTeamId == team.TeamId);

                team.WeeksLost = allSchedules.Count(s =>
                    (s.HomeTeamId == team.TeamId || s.AwayTeamId == team.TeamId) &&
                    s.WinningTeamId != team.TeamId);
            }

            return teams;
        }

        public async Task<TeamViewModel?> GetTeamByIdAsync(int id)
        {
            var team = await _context.Teams
                .Include(t => t.Captain)
                .Include(t => t.PlayerGames)
                .FirstOrDefaultAsync(t => t.TeamId == id);

            if (team == null) return null;

            return new TeamViewModel
            {
                TeamId = team.TeamId,
                TeamName = team.TeamName,
                CaptainPlayerId = team.CaptainPlayerId,
                CaptainName = team.Captain != null ? (string.IsNullOrEmpty(team.Captain.LastName) ? team.Captain.FirstName : $"{team.Captain.FirstName} {team.Captain.LastName}") : null,
                GamesPlayed = team.PlayerGames.Sum(pg => pg.Wins + pg.Losses),
                GamesWon = team.PlayerGames.Sum(pg => pg.Wins),
                GamesLost = team.PlayerGames.Sum(pg => pg.Losses)
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
                .Include(t => t.PlayerGames)
                .Select(t => new TeamStandingViewModel
                {
                    TeamId = t.TeamId,
                    TeamName = t.TeamName,
                    CaptainName = t.Captain != null ? (string.IsNullOrEmpty(t.Captain.LastName) ? t.Captain.FirstName : $"{t.Captain.FirstName} {t.Captain.LastName}") : null,
                    Wins = t.PlayerGames.Sum(pg => pg.Wins),
                    Losses = t.PlayerGames.Sum(pg => pg.Losses),
                    GamesPlayed = t.PlayerGames.Sum(pg => pg.Wins + pg.Losses),
                    Points = t.PlayerGames.Sum(pg => pg.Wins),
                    WinPercentage = (t.PlayerGames.Sum(pg => pg.Wins + pg.Losses)) > 0
                        ? (decimal)t.PlayerGames.Sum(pg => pg.Wins) / (t.PlayerGames.Sum(pg => pg.Wins + pg.Losses)) * 100
                        : 0
                })
                .OrderByDescending(ts => ts.Points)
                .ThenByDescending(ts => ts.WinPercentage)
                .ToListAsync();

            var allSchedules = await _context.Schedules.Where(s => s.WinningTeamId != null).ToListAsync();

            foreach (var team in standings)
            {
                team.WeeksWon = allSchedules.Count(s => s.WinningTeamId == team.TeamId);
            }

            for (int i = 0; i < standings.Count; i++)
            {
                standings[i].Rank = i + 1;
            }

            foreach (var team in standings)
            {
                if (team.Rank <= 3)
                {
                    team.Division = "Division A";
                }
                else if (team.Rank <= 7)
                {
                    team.Division = "Division B";
                }
                else
                {
                    team.Division = "Division C";
                }
            }

            return standings;
        }

        public async Task<List<PlayerViewModel>> GetTeamRosterAsync(int teamId)
        {
            return await _context.Players
                .Where(p => p.TeamId == teamId)
                .Include(p => p.PlayerGames)
                .Select(p => new PlayerViewModel
                {
                    PlayerId = p.PlayerId,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    TeamId = p.TeamId,
                    GamesPlayed = p.PlayerGames.Sum(pg => pg.Wins + pg.Losses),
                    GamesWon = p.PlayerGames.Sum(pg => pg.Wins),
                    GamesLost = p.PlayerGames.Sum(pg => pg.Losses)
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