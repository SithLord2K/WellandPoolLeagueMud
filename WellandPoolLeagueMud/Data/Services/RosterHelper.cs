using WellandPoolLeagueMud.Data.Models;
using WellandPoolLeagueMud.Data.ViewModels;

namespace WellandPoolLeagueMud.Data.Services
{
    public class RosterHelper(IDataFactory dataFactory)
    {
        public async Task<List<TeamRoster>> GetTeamRostersAsync()
        {
            var teams = await dataFactory.GetTeamsAsync();
            var players = await dataFactory.GetPlayersAsync();

            if (teams == null || players == null)
            {
                return new List<TeamRoster>();
            }

            var rosters = new List<TeamRoster>();
            foreach (var team in teams)
            {
                var teamPlayers = players.Where(p => p.TeamId == team.TeamId).ToList();
                var teamRoster = new TeamRoster
                {
                    TeamId = team.TeamId,
                    TeamName = team.TeamName,
                    Players = teamPlayers
                };
                rosters.Add(teamRoster);
            }

            return rosters;
        }

        public async Task<TeamRoster?> GetSingleTeamRosterAsync(int teamId)
        {
            var team = await dataFactory.GetSingleTeamAsync(teamId);
            if (team == null)
            {
                return null;
            }

            var players = await dataFactory.GetPlayersAsync();
            var teamPlayers = players?.Where(p => p.TeamId == team.TeamId).ToList();

            return new TeamRoster
            {
                TeamId = team.TeamId,
                TeamName = team.TeamName,
                Players = teamPlayers ?? new List<WPL_Player>()
            };
        }

        public async Task<List<WPL_Schedule>> GetSchedulesAsync()
        {
            return await dataFactory.GetSchedulesAsync();
        }
    }
}