using WellandPoolLeagueMud.Data.Models;

namespace WellandPoolLeagueMud.Data.Services
{
    public class PlayerHelpers(DataFactory dataFactory)
    {
        private readonly DataFactory dataFactory = dataFactory;

        public async Task<List<Players>> ConsolidatePlayersAsync()
        {
            var allPlayers = await dataFactory.GetPlayers();
            var allPlayerData = await dataFactory.GetAllPlayerDataAsync();

            if (allPlayers == null || allPlayerData == null)
            {
                return new List<Players>();
            }

            var playerTotalsLookup = allPlayerData.GroupBy(pd => pd.PlayerId)
                .ToDictionary(
                    group => group.Key,
                    group => new
                    {
                        GamesWon = group.Sum(x => x.GamesWon),
                        GamesLost = group.Sum(x => x.GamesLost),
                        GamesPlayed = group.Sum(x => x.GamesPlayed)
                    }
                );

            var pList = allPlayers
                .Select(p => {
                    playerTotalsLookup.TryGetValue(p.Id, out var totals);

                    return new Players
                    {
                        Id = p.Id,
                        TeamId = p.TeamId,
                        FirstName = p.FirstName ?? string.Empty,
                        LastName = p.LastName ?? string.Empty,
                        GamesWon = totals?.GamesWon ?? 0,
                        GamesLost = totals?.GamesLost ?? 0,
                        GamesPlayed = totals?.GamesPlayed ?? 0
                    };
                })
                .ToList();

            return pList;
        }

        public async Task<Players> GetPlayerDetails(int id)
        {
            var playerInfo = await dataFactory.GetSinglePlayer(id);
            var getPlayerData = await dataFactory.GetSinglePlayerData(id);

            if (playerInfo == null)
            {
                return new Players();
            }

            var playerTotals = new Players
            {
                Id = playerInfo.Id,
                FirstName = playerInfo.FirstName,
                LastName = playerInfo.LastName ?? string.Empty,
                GamesWon = getPlayerData?.Sum(gw => gw.GamesWon) ?? 0,
                GamesLost = getPlayerData?.Sum(y => y.GamesLost) ?? 0,
                GamesPlayed = getPlayerData?.Sum(y => y.GamesPlayed) ?? 0
            };

            return playerTotals;
        }

        public async Task<TeamStats> GetTeamStatsAsync()
        {
            var weekTotals = await dataFactory.GetAllWeeks();
            var teamTotals = await dataFactory.GetAllPlayerDataAsync();

            var teamStats = new TeamStats
            {
                TotalGamesWon = teamTotals?.Sum(x => x.GamesWon) ?? 0,
                TotalGamesLost = teamTotals?.Sum(y => y.GamesLost) ?? 0,
                WeeksPlayed = weekTotals?.Count ?? 0
            };

            teamStats.TotalGamesPlayed = teamStats.TotalGamesWon + teamStats.TotalGamesLost;

            if (teamStats.TotalGamesPlayed > 0)
            {
                teamStats.TotalAverage = decimal.Round(teamStats.TotalGamesWon / (decimal)teamStats.TotalGamesPlayed, 2);
            }
            else
            {
                teamStats.TotalAverage = 0;
            }

            return teamStats;
        }

        public async Task<List<TeamDetail>> GetTeamDetailsAsync()
        {
            var teamDetails = await dataFactory.GetTeamDetails();

            if (teamDetails == null)
            {
                return new List<TeamDetail>();
            }

            return teamDetails.Where(x => x.TeamName != "Bye").ToList();
        }
    }
}