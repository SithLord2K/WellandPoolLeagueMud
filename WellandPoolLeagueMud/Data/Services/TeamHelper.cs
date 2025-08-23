using WellandPoolLeagueMud.Data.Models;
using WellandPoolLeagueMud.Data.Services;
using WellandPoolLeagueMud.Data.ViewModels;

namespace WellandPoolLeagueMud.Data.Services
{
    public class TeamHelper(IDataFactory dataFactory)
    {
        public async Task<List<TeamStatsViewModel>> GetAllTeamStatsAsync()
        {
            var teams = await dataFactory.GetTeamsAsync();
            var allSchedules = await dataFactory.GetSchedulesAsync();
            var allPlayerGames = await dataFactory.GetAllPlayerGamesAsync();
            var allWeeklyWinners = await dataFactory.GetWeeklyWinnersAsync();

            if (teams == null || allSchedules == null || allPlayerGames == null || allWeeklyWinners == null)
            {
                return new List<TeamStatsViewModel>();
            }

            var teamPlayerGamesLookup = allPlayerGames
                .Where(pg => pg.Player?.TeamId != null)
                .GroupBy(pg => pg.Player!.TeamId)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        FramesWon = g.Sum(p => p.FramesWon),
                        FramesLost = g.Sum(p => p.FramesLost)
                    }
                );

            var weeksWonLookup = allWeeklyWinners
                .GroupBy(w => w.WinningTeamId)
                .ToDictionary(g => g.Key, g => g.Count());

            var weeksPlayedCount = allSchedules.Select(s => s.WeekNumber).Distinct().Count();

            var allTeamStats = teams.Where(t => t.TeamName != "Bye")
                .Select(team =>
                {
                    teamPlayerGamesLookup.TryGetValue(team.TeamId, out var totals);
                    weeksWonLookup.TryGetValue(team.TeamId, out var weeksWon);

                    int totalFramesWon = totals?.FramesWon ?? 0;
                    int totalFramesLost = totals?.FramesLost ?? 0;
                    int totalWeeksWon = weeksWon;
                    int weeksLost = weeksPlayedCount - totalWeeksWon;
                    int totalFramesPlayed = totalFramesWon + totalFramesLost;

                    decimal average = (totalFramesPlayed > 0) ? (decimal)totalFramesWon / totalFramesPlayed : 0;
                    decimal points = totalWeeksWon * 2;

                    return new TeamStatsViewModel
                    {
                        TeamName = team.TeamName,
                        TotalFramesWon = totalFramesWon,
                        TotalFramesLost = totalFramesLost,
                        WeeksWon = totalWeeksWon,
                        WeeksLost = weeksLost,
                        WeeksPlayed = weeksPlayedCount,
                        TotalFramesPlayed = totalFramesPlayed,
                        TotalAverage = decimal.Round(average, 3),
                        Points = points
                    };
                })
                .OrderByDescending(t => t.WeeksWon)
                .ThenByDescending(t => t.TotalFramesWon)
                .ToList();

            return allTeamStats;
        }
    }
}