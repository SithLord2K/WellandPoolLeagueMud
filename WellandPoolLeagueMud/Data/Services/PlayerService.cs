using Microsoft.EntityFrameworkCore;
using WellandPoolLeagueMud.Data.Models;
using WellandPoolLeagueMud.Data.ViewModels;

namespace WellandPoolLeagueMud.Data.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly WellandPoolLeagueDbContext _context;

        public PlayerService(WellandPoolLeagueDbContext context)
        {
            _context = context;
        }

        public async Task<List<PlayerViewModel>> GetAllPlayersAsync()
        {
            return await _context.Players
                .Include(p => p.PlayerGames)
                .Include(p => p.Team)
                .Select(p => new PlayerViewModel
                {
                    PlayerId = p.PlayerId,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    TeamId = p.TeamId,
                    TeamName = p.Team != null ? p.Team.TeamName : null,
                    GamesPlayed = p.PlayerGames.Sum(pg => pg.Wins + pg.Losses),
                    GamesWon = p.PlayerGames.Sum(pg => pg.Wins),
                    GamesLost = p.PlayerGames.Sum(pg => pg.Losses)
                })
                .OrderBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .ToListAsync();
        }

        public async Task<PlayerViewModel?> GetPlayerByIdAsync(int id)
        {
            var player = await _context.Players
                .Include(p => p.PlayerGames)
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.PlayerId == id);

            if (player == null) return null;

            return new PlayerViewModel
            {
                PlayerId = player.PlayerId,
                FirstName = player.FirstName,
                LastName = player.LastName,
                TeamId = player.TeamId,
                TeamName = player.Team?.TeamName,
                GamesPlayed = player.PlayerGames.Sum(pg => pg.Wins + pg.Losses),
                GamesWon = player.PlayerGames.Sum(pg => pg.Wins),
                GamesLost = player.PlayerGames.Sum(pg => pg.Losses)
            };
        }

        public async Task<PlayerViewModel> CreatePlayerAsync(PlayerViewModel playerVM)
        {
            var player = new Player
            {
                FirstName = playerVM.FirstName,
                LastName = playerVM.LastName,
                TeamId = playerVM.TeamId
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            playerVM.PlayerId = player.PlayerId;
            return playerVM;
        }

        public async Task<PlayerViewModel?> UpdatePlayerAsync(PlayerViewModel playerVM)
        {
            var player = await _context.Players.FindAsync(playerVM.PlayerId);
            if (player == null) return null;

            player.FirstName = playerVM.FirstName;
            player.LastName = playerVM.LastName;
            player.TeamId = playerVM.TeamId;

            await _context.SaveChangesAsync();
            return playerVM;
        }

        public async Task<bool> DeletePlayerAsync(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null) return false;

            _context.Players.Remove(player);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PlayerStandingViewModel>> GetPlayerStandingsAsync()
        {
            var standings = await _context.Players
                .Include(p => p.PlayerGames)
                .Include(p => p.Team)
                .Select(p => new
                {
                    PlayerId = p.PlayerId,
                    PlayerName = string.IsNullOrEmpty(p.LastName) ? p.FirstName : $"{p.FirstName} {p.LastName}",
                    TeamName = p.Team != null ? p.Team.TeamName : null,
                    Wins = p.PlayerGames.Sum(pg => pg.Wins),
                    Losses = p.PlayerGames.Sum(pg => pg.Losses)
                })
                .Select(p => new PlayerStandingViewModel
                {
                    PlayerId = p.PlayerId,
                    PlayerName = p.PlayerName,
                    TeamName = p.TeamName,
                    Wins = p.Wins,
                    Losses = p.Losses,
                    GamesPlayed = p.Wins + p.Losses,
                    WinPercentage = (p.Wins + p.Losses) > 0 ? (decimal)p.Wins / (p.Wins + p.Losses) * 100 : 0
                })
                .OrderByDescending(ps => ps.WinPercentage)
                .ThenByDescending(ps => ps.Wins)
                .ToListAsync();

            for (int i = 0; i < standings.Count; i++)
            {
                standings[i].Rank = i + 1;
            }

            return standings;
        }

        public async Task<bool> PlayerExistsAsync(int id)
        {
            return await _context.Players.AnyAsync(p => p.PlayerId == id);
        }

        public async Task<List<PlayerViewModel>> GetPlayersByTeamAsync(int teamId)
        {
            return await _context.Players
                .Include(p => p.PlayerGames)
                .Include(p => p.Team)
                .Where(p => p.TeamId == teamId)
                .Select(p => new PlayerViewModel
                {
                    PlayerId = p.PlayerId,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    TeamId = p.TeamId,
                    TeamName = p.Team!.TeamName,
                    GamesPlayed = p.PlayerGames.Sum(pg => pg.Wins + pg.Losses),
                    GamesWon = p.PlayerGames.Sum(pg => pg.Wins),
                    GamesLost = p.PlayerGames.Sum(pg => pg.Losses)
                })
                .OrderBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .ToListAsync();
        }
    }
}