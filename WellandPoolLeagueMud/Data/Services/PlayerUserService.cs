using Microsoft.EntityFrameworkCore;
using WellandPoolLeagueMud.Data; // Assuming DbContext is in this namespace
using WellandPoolLeagueMud.Data.Models;

namespace WellandPoolLeagueMud.Data.Services
{
    public class PlayerUserService : IPlayerUserService
    {
        private readonly WellandPoolLeagueDbContext _context;
        private readonly ILogger<PlayerUserService> _logger;

        public PlayerUserService(WellandPoolLeagueDbContext context, ILogger<PlayerUserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // This is the new, more performant method to get all linked players at once.
        public async Task<Dictionary<string, Player>> GetAllPlayerLinksAsync()
        {
            try
            {
                var linkedPlayers = await _context.Players
                    .Include(p => p.Team) // Eagerly load Team data
                    .Where(p => !string.IsNullOrEmpty(p.Auth0UserId))
                    .ToListAsync();

                // The '!' is safe here because the Where clause guarantees Auth0UserId is not null.
                return linkedPlayers.ToDictionary(p => p.Auth0UserId!, p => p);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all player links.");
                return new Dictionary<string, Player>(); // Return empty on error
            }
        }

        public async Task<List<Player>> GetUnlinkedPlayersAsync()
        {
            return await _context.Players
                .Include(p => p.Team)
                .Where(p => string.IsNullOrEmpty(p.Auth0UserId))
                .OrderBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .ToListAsync();
        }

        public async Task<Player?> GetPlayerForUserAsync(string auth0UserId)
        {
            return await _context.Players
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.Auth0UserId == auth0UserId);
        }

        public async Task<bool> LinkUserToPlayerAsync(string auth0UserId, int playerId)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player == null || !string.IsNullOrEmpty(player.Auth0UserId))
            {
                // Player not found or already linked
                return false;
            }

            player.Auth0UserId = auth0UserId;
            _context.Players.Update(player);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UnlinkUserFromPlayerAsync(string auth0UserId)
        {
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Auth0UserId == auth0UserId);
            if (player == null)
            {
                return false; // No player linked to this user
            }

            player.Auth0UserId = null;
            _context.Players.Update(player);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}