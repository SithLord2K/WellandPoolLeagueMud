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
                .Select(p => new PlayerViewModel
                {
                    PlayerId = p.PlayerId,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Phone = p.Phone,
                    GamesPlayed = p.PlayerGames.Count,
                    GamesWon = p.PlayerGames.Count(pg => pg.IsWin),
                    GamesLost = p.PlayerGames.Count(pg => !pg.IsWin)
                })
                .OrderBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .ToListAsync();
        }

        public async Task<PlayerViewModel?> GetPlayerByIdAsync(int id)
        {
            var player = await _context.Players
                .Include(p => p.PlayerGames)
                .FirstOrDefaultAsync(p => p.PlayerId == id);

            if (player == null) return null;

            return new PlayerViewModel
            {
                PlayerId = player.PlayerId,
                FirstName = player.FirstName,
                LastName = player.LastName,
                Phone = player.Phone,
                GamesPlayed = player.PlayerGames.Count,
                GamesWon = player.PlayerGames.Count(pg => pg.IsWin),
                GamesLost = player.PlayerGames.Count(pg => !pg.IsWin)
            };
        }

        public async Task<PlayerViewModel> CreatePlayerAsync(PlayerViewModel playerVM)
        {
            var player = new Player
            {
                FirstName = playerVM.FirstName,
                LastName = playerVM.LastName,
                Phone = playerVM.Phone
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
            player.Phone = playerVM.Phone;

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
                .ThenInclude(pg => pg.Team)
                .Select(p => new PlayerStandingViewModel
                {
                    PlayerId = p.PlayerId,
                    PlayerName = string.IsNullOrEmpty(p.LastName) ? p.FirstName : $"{p.FirstName} {p.LastName}",
                    TeamName = p.PlayerGames.OrderByDescending(pg => pg.GameDate).FirstOrDefault().Team.TeamName,
                    GamesPlayed = p.PlayerGames.Count,
                    Wins = p.PlayerGames.Count(pg => pg.IsWin),
                    Losses = p.PlayerGames.Count(pg => !pg.IsWin),
                    WinPercentage = p.PlayerGames.Count > 0 ? (decimal)p.PlayerGames.Count(pg => pg.IsWin) / p.PlayerGames.Count * 100 : 0
                })
                .OrderByDescending(ps => ps.WinPercentage)
                .ThenByDescending(ps => ps.Wins)
                .ToListAsync();

            // Add ranks
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
    }
}