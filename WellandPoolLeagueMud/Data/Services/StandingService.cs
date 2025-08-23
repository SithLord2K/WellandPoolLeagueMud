// Data/Services/StandingService.cs

using WellandPoolLeagueMud.Data.Models;
using WellandPoolLeagueMud.Data.ViewModels;

namespace WellandPoolLeagueMud.Data.Services
{
    public class StandingService(IDataFactory dataFactory)
    {
        public async Task<List<TeamStandingViewModel>> GetTeamStandingsAsync()
        {
            var allTeams = await dataFactory.GetTeamsAsync();
            var allSchedules = await dataFactory.GetSchedulesAsync();
            var allWeeklyWinners = await dataFactory.GetWeeklyWinnersAsync();
            var allPlayerGames = await dataFactory.GetAllPlayerGamesAsync();

            if (allTeams == null || allSchedules == null || allWeeklyWinners == null || allPlayerGames == null)
            {
                return new List<TeamStandingViewModel>();
            }

            var weeksPlayedCount = allSchedules.Select(s => s.WeekNumber).Distinct().Count();

            var weeklyWinsLookup = allWeeklyWinners
                .GroupBy(w => w.WinningTeamId)
                .ToDictionary(g => g.Key, g => g.Count());

            // Replace the incorrect playerGamesLookup block with the following:

            var playerGamesLookup = allPlayerGames
                .Where(pg => pg.Player != null)
                .GroupBy(pg => pg.Player!.TeamId)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        FramesWon = g.Sum(p => p.FramesWon),
                        FramesLost = g.Sum(p => p.FramesLost)
                    }
                );

            var standings = allTeams
                .Where(t => t.TeamName != "Bye")
                .Select(team =>
                {
                    weeklyWinsLookup.TryGetValue(team.TeamId, out var weeksWon);
                    playerGamesLookup.TryGetValue(team.TeamId, out var totals);

                    var totalFramesWon = totals?.FramesWon ?? 0;
                    var totalFramesLost = totals?.FramesLost ?? 0;
                    var weeksLost = weeksPlayedCount - weeksWon;
                    var points = weeksWon * 2;

                    return new TeamStandingViewModel
                    {
                        TeamId = team.TeamId,
                        TeamName = team.TeamName,
                        WeeksWon = weeksWon,
                        WeeksLost = weeksLost,
                        TotalFramesWon = totalFramesWon,
                        TotalFramesLost = totalFramesLost,
                        Points = points,
                        Rank = 0 // Will be calculated later
                    };
                })
                .OrderByDescending(t => t.Points)
                .ThenByDescending(t => t.TotalFramesWon)
                .ToList();

            // Assign ranks and divisions
            for (int i = 0; i < standings.Count; i++)
            {
                standings[i].Rank = i + 1;
                standings[i].Division = GetDivision(i + 1);
            }

            return standings;
        }

        private string GetDivision(int rank)
        {
            if (rank >= 1 && rank <= 3) return "Division A";
            if (rank >= 4 && rank <= 6) return "Division B";
            return "Division C";
        }
    }
}