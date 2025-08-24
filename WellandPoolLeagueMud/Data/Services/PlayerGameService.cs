// Data/Services/PlayerGameService.cs
using Microsoft.EntityFrameworkCore;
using WellandPoolLeagueMud.Data.Models;
using WellandPoolLeagueMud.Data.ViewModels;

namespace WellandPoolLeagueMud.Data.Services
{
    public class PlayerGameService : IPlayerGameService
    {
        private readonly WellandPoolLeagueDbContext _context;

        public PlayerGameService(WellandPoolLeagueDbContext context)
        {
            _context = context;
        }

        public async Task<List<PlayerGameViewModel>> GetAllPlayerGamesAsync()
        {
            return await _context.PlayerGames
                .Include(pg => pg.Player)
                .Include(pg => pg.Team)
                .Select(pg => new PlayerGameViewModel
                {
                    PlayerGameId = pg.PlayerGameId,
                    PlayerId = pg.PlayerId,
                    PlayerName = string.IsNullOrEmpty(pg.Player.LastName) ? pg.Player.FirstName : $"{pg.Player.FirstName} {pg.Player.LastName}",
                    TeamId = pg.TeamId,
                    TeamName = pg.Team.TeamName,
                    WeekNumber = pg.WeekNumber,
                    IsWin = pg.IsWin,
                    GameDate = pg.GameDate,
                    Notes = pg.Notes
                })
                .OrderBy(pg => pg.WeekNumber)
                .ThenBy(pg => pg.GameDate)
                .ToListAsync();
        }

        public async Task<PlayerGameViewModel?> GetPlayerGameByIdAsync(int id)
        {
            var playerGame = await _context.PlayerGames
                .Include(pg => pg.Player)
                .Include(pg => pg.Team)
                .FirstOrDefaultAsync(pg => pg.PlayerGameId == id);

            if (playerGame == null) return null;

            return new PlayerGameViewModel
            {
                PlayerGameId = playerGame.PlayerGameId,
                PlayerId = playerGame.PlayerId,
                PlayerName = string.IsNullOrEmpty(playerGame.Player.LastName) ? playerGame.Player.FirstName : $"{playerGame.Player.FirstName} {playerGame.Player.LastName}",
                TeamId = playerGame.TeamId,
                TeamName = playerGame.Team.TeamName,
                WeekNumber = playerGame.WeekNumber,
                IsWin = playerGame.IsWin,
                GameDate = playerGame.GameDate,
                Notes = playerGame.Notes
            };
        }

        public async Task<PlayerGameViewModel> CreatePlayerGameAsync(PlayerGameViewModel playerGameVM)
        {
            var playerGame = new PlayerGame
            {
                PlayerId = playerGameVM.PlayerId,
                TeamId = playerGameVM.TeamId,
                WeekNumber = playerGameVM.WeekNumber,
                IsWin = playerGameVM.IsWin,
                GameDate = playerGameVM.GameDate,
                Notes = playerGameVM.Notes
            };

            _context.PlayerGames.Add(playerGame);
            await _context.SaveChangesAsync();

            playerGameVM.PlayerGameId = playerGame.PlayerGameId;
            return playerGameVM;
        }

        public async Task<PlayerGameViewModel?> UpdatePlayerGameAsync(PlayerGameViewModel playerGameVM)
        {
            var playerGame = await _context.PlayerGames.FindAsync(playerGameVM.PlayerGameId);
            if (playerGame == null) return null;

            playerGame.PlayerId = playerGameVM.PlayerId;
            playerGame.TeamId = playerGameVM.TeamId;
            playerGame.WeekNumber = playerGameVM.WeekNumber;
            playerGame.IsWin = playerGameVM.IsWin;
            playerGame.GameDate = playerGameVM.GameDate;
            playerGame.Notes = playerGameVM.Notes;

            await _context.SaveChangesAsync();
            return playerGameVM;
        }

        public async Task<bool> DeletePlayerGameAsync(int id)
        {
            var playerGame = await _context.PlayerGames.FindAsync(id);
            if (playerGame == null) return false;

            _context.PlayerGames.Remove(playerGame);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PlayerGameViewModel>> GetPlayerGamesByWeekAsync(int weekNumber)
        {
            return await _context.PlayerGames
                .Include(pg => pg.Player)
                .Include(pg => pg.Team)
                .Where(pg => pg.WeekNumber == weekNumber)
                .Select(pg => new PlayerGameViewModel
                {
                    PlayerGameId = pg.PlayerGameId,
                    PlayerId = pg.PlayerId,
                    PlayerName = string.IsNullOrEmpty(pg.Player.LastName) ? pg.Player.FirstName : $"{pg.Player.FirstName} {pg.Player.LastName}",
                    TeamId = pg.TeamId,
                    TeamName = pg.Team.TeamName,
                    WeekNumber = pg.WeekNumber,
                    IsWin = pg.IsWin,
                    GameDate = pg.GameDate,
                    Notes = pg.Notes
                })
                .OrderBy(pg => pg.GameDate)
                .ThenBy(pg => pg.TeamName)
                .ToListAsync();
        }

        public async Task<List<PlayerGameViewModel>> GetPlayerGamesByPlayerAsync(int playerId)
        {
            return await _context.PlayerGames
                .Include(pg => pg.Player)
                .Include(pg => pg.Team)
                .Where(pg => pg.PlayerId == playerId)
                .Select(pg => new PlayerGameViewModel
                {
                    PlayerGameId = pg.PlayerGameId,
                    PlayerId = pg.PlayerId,
                    PlayerName = string.IsNullOrEmpty(pg.Player.LastName) ? pg.Player.FirstName : $"{pg.Player.FirstName} {pg.Player.LastName}",
                    TeamId = pg.TeamId,
                    TeamName = pg.Team.TeamName,
                    WeekNumber = pg.WeekNumber,
                    IsWin = pg.IsWin,
                    GameDate = pg.GameDate,
                    Notes = pg.Notes
                })
                .OrderByDescending(pg => pg.GameDate)
                .ThenBy(pg => pg.WeekNumber)
                .ToListAsync();
        }

        public async Task<List<PlayerGameViewModel>> GetPlayerGamesByTeamAsync(int teamId)
        {
            return await _context.PlayerGames
                .Include(pg => pg.Player)
                .Include(pg => pg.Team)
                .Where(pg => pg.TeamId == teamId)
                .Select(pg => new PlayerGameViewModel
                {
                    PlayerGameId = pg.PlayerGameId,
                    PlayerId = pg.PlayerId,
                    PlayerName = string.IsNullOrEmpty(pg.Player.LastName) ? pg.Player.FirstName : $"{pg.Player.FirstName} {pg.Player.LastName}",
                    TeamId = pg.TeamId,
                    TeamName = pg.Team.TeamName,
                    WeekNumber = pg.WeekNumber,
                    IsWin = pg.IsWin,
                    GameDate = pg.GameDate,
                    Notes = pg.Notes
                })
                .OrderBy(pg => pg.WeekNumber)
                .ThenBy(pg => pg.GameDate)
                .ThenBy(pg => pg.PlayerName)
                .ToListAsync();
        }
    }
}