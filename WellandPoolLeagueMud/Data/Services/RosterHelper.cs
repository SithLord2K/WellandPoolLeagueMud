using WellandPoolLeagueMud.Data.Models;
using Roster = WellandPoolLeagueMud.Data.Models.Roster;

namespace WellandPoolLeagueMud.Data.Services
{
    public class RosterHelper
    {
        private readonly DataFactory dataFactory;

        public RosterHelper(DataFactory dataFactory)
        {
            this.dataFactory = dataFactory;
        }

        public async Task<List<Roster>> GetRoster()
        {
            var teams = await dataFactory.GetTeamDetails();
            var players = await dataFactory.GetPlayers();

            if (teams == null || players == null)
            {
                return new List<Roster>();
            }

            var rosters = teams.SelectMany(team =>
                players.Where(player => player.TeamId == team.Id)
                    .Select(player => new Roster
                    {
                        TeamId = team.Id,
                        TeamName = team.TeamName,
                        Captain_Player_Id = team.Captain_Player_Id,
                        Player_Id = player.Id,
                        Player_Name = $"{player.FirstName} {player.LastName}"
                    })
            ).ToList();

            return rosters;
        }

        public async Task<List<Roster>> GetSingleTeamRoster(int teamId)
        {
            var team = await dataFactory.GetSingleTeam(teamId);
            var players = await dataFactory.GetPlayers();

            if (team == null || players == null)
            {
                return new List<Roster>();
            }

            var play = players.Where(x => x.TeamId == team.Id).ToList();

            var rosters = play.Select(player => new Roster
            {
                TeamId = team.Id,
                TeamName = team.TeamName,
                Captain_Player_Id = team.Captain_Player_Id,
                Player_Id = player.Id,
                Player_Name = $"{player.FirstName} {player.LastName}",
                IsCaptain = player.Id == team.Captain_Player_Id
            }).ToList();

            return rosters;
        }

        public async Task<List<Schedule>> GetSchedules()
        {
            return await dataFactory.GetSchedule();
        }
    }
}