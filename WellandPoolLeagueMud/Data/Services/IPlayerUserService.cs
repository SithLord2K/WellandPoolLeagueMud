using WellandPoolLeagueMud.Data.Models;

namespace WellandPoolLeagueMud.Data.Services
{
    public interface IPlayerUserService
    {
        Task<List<Player>> GetUnlinkedPlayersAsync();
        Task<Player?> GetPlayerForUserAsync(string auth0UserId);
        Task<bool> LinkUserToPlayerAsync(string auth0UserId, int playerId);
        Task<bool> UnlinkUserFromPlayerAsync(string auth0UserId);
        Task<Dictionary<string, Player>> GetAllPlayerLinksAsync();
    }
}