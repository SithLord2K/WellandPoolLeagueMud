using WellandPoolLeagueMud.Data.Models;
using WellandPoolLeagueMud.Data.Services;

namespace WellandPoolLeagueMud.Data.Services
{
    public class PlayerHelpers(IDataFactory dataFactory)
    {
        public async Task<List<ConsolidatedPlayer>> ConsolidatePlayersAsync()
        {
            var allPlayers = await dataFactory.GetPlayersAsync();
            var allPlayerGames = await dataFactory.GetAllPlayerGamesAsync();

            if (allPlayers == null || !allPlayers.Any())
            {
                return new List<ConsolidatedPlayer>();
            }

            var playerGamesLookup = allPlayerGames.GroupBy(pg => pg.PlayerId)
                .ToDictionary(
                    group => group.Key,
                    group => new
                    {
                        FramesWon = group.Sum(x => x.FramesWon),
                        FramesLost = group.Sum(x => x.FramesLost)
                    }
                );

            var consolidatedPlayers = allPlayers
                .Select(p =>
                {
                    playerGamesLookup.TryGetValue(p.PlayerId, out var totals);

                    return new ConsolidatedPlayer
                    {
                        PlayerId = p.PlayerId,
                        TeamId = p.TeamId,
                        FirstName = p.FirstName,
                        LastName = p.LastName,
                        FramesWon = totals?.FramesWon ?? 0,
                        FramesLost = totals?.FramesLost ?? 0
                    };
                })
                .OrderByDescending(p => p.Average)
                .ToList();

            return consolidatedPlayers;
        }

        public async Task<ConsolidatedPlayer?> GetPlayerDetailsAsync(int id)
        {
            var playerInfo = await dataFactory.GetSinglePlayerAsync(id);

            if (playerInfo == null)
            {
                return null;
            }

            var playerGames = await dataFactory.GetPlayerGamesByPlayerIdAsync(id);
            var framesWon = playerGames.Sum(pg => pg.FramesWon);
            var framesLost = playerGames.Sum(pg => pg.FramesLost);

            return new ConsolidatedPlayer
            {
                PlayerId = playerInfo.PlayerId,
                FirstName = playerInfo.FirstName,
                LastName = playerInfo.LastName,
                TeamId = playerInfo.TeamId,
                FramesWon = framesWon,
                FramesLost = framesLost
            };
        }

        public async Task<TeamStats> GetTeamStatsAsync()
        {
            var allPlayerGames = await dataFactory.GetAllPlayerGamesAsync();
            var allSchedules = await dataFactory.GetSchedulesAsync();

            var totalFramesWon = allPlayerGames?.Sum(pg => pg.FramesWon) ?? 0;
            var totalFramesLost = allPlayerGames?.Sum(pg => pg.FramesLost) ?? 0;
            var weeksPlayed = allSchedules?.Select(s => s.WeekNumber).Distinct().Count() ?? 0;

            var totalFramesPlayed = totalFramesWon + totalFramesLost;
            var totalAverage = totalFramesPlayed > 0 ?
                decimal.Round((decimal)totalFramesWon / totalFramesPlayed, 2) : 0;

            return new TeamStats
            {
                TotalFramesWon = totalFramesWon,
                TotalFramesLost = totalFramesLost,
                WeeksPlayed = weeksPlayed,
                TotalAverage = totalAverage
            };
        }

        public async Task<List<WPL_Team>> GetActiveTeamsAsync()
        {
            var teams = await dataFactory.GetTeamsAsync();
            return teams?.Where(t => t.TeamName != "Bye").ToList() ?? new List<WPL_Team>();
        }
    }
}