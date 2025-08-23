using WellandPoolLeagueMud.Data.Models;
using WellandPoolLeagueMud.Data.Services;
using WellandPoolLeagueMud.Data.ViewModels;

namespace WellandPoolLeagueMud.Data.Services
{
    public class WeekHelper(IDataFactory dataFactory)
    {
        public async Task<List<WeekViewModel>> GetWeeksFullInfoAsync()
        {
            var allSchedules = await dataFactory.GetSchedulesAsync();
            var allTeams = await dataFactory.GetTeamsAsync();

            if (allSchedules == null || !allSchedules.Any() || allTeams == null || !allTeams.Any())
            {
                return new List<WeekViewModel>();
            }

            var teamLookup = allTeams.ToDictionary(t => t.TeamId, t => t.TeamName);

            var weeksFullInfo = allSchedules
                .Select(schedule => new WeekViewModel
                {
                    WeekNumber = schedule.WeekNumber,
                    Date = schedule.Date,
                    HomeTeamId = schedule.HomeTeamId,
                    AwayTeamId = schedule.AwayTeamId,
                    HomeTeamName = teamLookup.TryGetValue(schedule.HomeTeamId, out var homeName) ? homeName : string.Empty,
                    AwayTeamName = teamLookup.TryGetValue(schedule.AwayTeamId, out var awayName) ? awayName : string.Empty,
                    WinningTeamId = schedule.WinningTeamId,
                    IsForfeit = schedule.Forfeit,
                    IsPlayoff = schedule.WeekNumber > 18,
                    TableNumber = schedule.TableNumber ?? 0
                })
                .OrderBy(w => w.WeekNumber)
                .ToList();

            return weeksFullInfo;
        }
    }
}