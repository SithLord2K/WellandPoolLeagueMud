using WellandPoolLeagueMud.Data.Models;
using WellandPoolLeagueMud.Data.Services;
using WellandPoolLeagueMud.Data.ViewModels;

namespace WellandPoolLeagueMud.Data.Services
{
    public class PDataService(IDataFactory dataFactory)
    {
        // This constant should be a variable or defined in a central location.
        // For this refactor, we will assume it's still InactiveTeamId = 12.
        private const int InactiveTeamId = 12;

        public async Task<List<PlayerViewModel>> GetFullPlayerDataAsync()
        {
            // Fetch all players and teams once for efficiency
            var allPlayers = await dataFactory.GetPlayersAsync();
            var allTeams = await dataFactory.GetTeamsAsync();
            var allPlayerGames = await dataFactory.GetAllPlayerGamesAsync();

            if (allPlayers == null || !allPlayers.Any() || allTeams == null)
            {
                return new List<PlayerViewModel>();
            }

            // Create a lookup for team names from the new WPL_Team model
            var teamNameLookup = allTeams.ToDictionary(t => t.TeamId, t => t.TeamName);

            // Group player games to calculate totals
            var playerGamesLookup = allPlayerGames.GroupBy(pg => pg.PlayerId)
                .ToDictionary(
                    group => group.Key,
                    group => new
                    {
                        FramesWon = group.Sum(x => x.FramesWon),
                        FramesLost = group.Sum(x => x.FramesLost)
                    }
                );

            var fullPlayerData = allPlayers
                .Where(player => player.TeamId != InactiveTeamId)
                .Select(player =>
                {
                    playerGamesLookup.TryGetValue(player.PlayerId, out var totals);

                    int framesWon = totals?.FramesWon ?? 0;
                    int framesLost = totals?.FramesLost ?? 0;
                    int gamesPlayed = framesWon + framesLost;
                    decimal average = gamesPlayed > 0 ? (decimal)framesWon / gamesPlayed : 0;

                    teamNameLookup.TryGetValue(player.TeamId, out var teamName);

                    return new PlayerViewModel
                    {
                        PlayerId = player.PlayerId,
                        FirstName = player.FirstName,
                        LastName = player.LastName,
                        FramesWon = framesWon,
                        FramesLost = framesLost,
                        GamesPlayed = gamesPlayed,
                        TeamName = teamName ?? "Unknown Team",
                        Average = decimal.Round(average, 3)
                    };
                })
                .OrderBy(p => p.TeamName)
                .ThenBy(p => p.LastName)
                .ToList();

            return fullPlayerData;
        }
    }
}