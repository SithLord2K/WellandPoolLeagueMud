using WellandPoolLeagueMud.Data.Models;
using System.Linq;

namespace WellandPoolLeagueMud.Data.Services
{
    public class PDataService
    {
        private readonly TeamHelper _teamHelper;
        private readonly PlayerHelpers _playerHelper;

        private const int InactiveTeamId = 12;

        public PDataService(TeamHelper teamHelper, PlayerHelpers playerHelper)
        {
            _teamHelper = teamHelper;
            _playerHelper = playerHelper;
        }

        public async Task<List<PDataModel>> GetFullPlayerData()
        {
            var teamDetails = await _playerHelper.GetTeamDetailsAsync();
            var playerData = await _playerHelper.ConsolidatePlayersAsync();

            if (teamDetails == null || playerData == null)
            {
                return new List<PDataModel>();
            }

            var teamNameLookup = teamDetails.ToDictionary(td => td.Id, td => td.TeamName);

            var pData = playerData
                .Where(player => player.TeamId != InactiveTeamId)
                .Select(player =>
                {
                    decimal average = 0;
                    if (player.GamesPlayed > 0)
                    {
                        average = player.GamesWon / (decimal)player.GamesPlayed;
                    }

                    return new PDataModel
                    {
                        FirstName = player.FirstName,
                        LastName = player.LastName,
                        GamesWon = player.GamesWon,
                        GamesLost = player.GamesLost,
                        GamesPlayed = player.GamesPlayed,
                        TeamName = teamNameLookup.TryGetValue(player.TeamId, out var teamName) ? teamName : "Unknown Team",
                        Average = average
                    };
                })
                .ToList();

            return pData;
        }
    }
}