using WellandPoolLeagueMud.Data.Models;

namespace WellandPoolLeagueMud.Data.Services
{
    public class TeamHelper(DataFactory dataFactory, PlayerHelpers playerHelpers)
    {
        private readonly DataFactory dataFactory = dataFactory;
        private readonly PlayerHelpers playerHelpers = playerHelpers;

        public async Task<List<TeamStats>> GetAllTeamStats()
        {
            var teams = await dataFactory.GetTeamDetails();
            var weekTotals = await dataFactory.GetAllWeeks();
            var teamTotals = await playerHelpers.ConsolidatePlayersAsync();

            if (teams == null || weekTotals == null || teamTotals == null)
            {
                return new List<TeamStats>();
            }

            var teamTotalsLookup = teamTotals
                .GroupBy(p => p.TeamId)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        GamesWon = g.Sum(p => p.GamesWon),
                        GamesLost = g.Sum(p => p.GamesLost)
                    }
                );

            var weeksWonLookup = weekTotals
                .Where(w => w.WinningTeamId.HasValue)
                .GroupBy(w => w.WinningTeamId.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            var allTeamStats = teams.Where(t => t.TeamName != "Bye")
                .Select(team =>
                {
                    teamTotalsLookup.TryGetValue(team.Id, out var totals);
                    weeksWonLookup.TryGetValue(team.Id, out var weeksWon);

                    int totalGamesWon = totals?.GamesWon ?? 0;
                    int totalGamesLost = totals?.GamesLost ?? 0;
                    int totalWeeksWon = weeksWon;
                    int weeksPlayed = weekTotals.Count;
                    int totalGamesPlayed = totalGamesWon + totalGamesLost;

                    decimal average = 0;
                    if (weeksPlayed > 0)
                    {
                        average = (decimal)totalWeeksWon / weeksPlayed;
                    }

                    return new TeamStats
                    {
                        TeamName = team.TeamName,
                        TotalGamesWon = totalGamesWon,
                        TotalGamesLost = totalGamesLost,
                        WeeksWon = totalWeeksWon,
                        WeeksLost = weeksPlayed - totalWeeksWon,
                        WeeksPlayed = weeksPlayed,
                        TotalGamesPlayed = totalGamesPlayed,
                        TotalAverage = average
                    };
                })
                .ToList();

            return allTeamStats;
        }
    }
}