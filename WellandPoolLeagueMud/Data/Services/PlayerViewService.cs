using WellandPoolLeagueMud.Data.Models;

namespace WellandPoolLeagueMud.Data.Services
{
    public class PlayerViewService
    {
        private readonly DataFactory dataFactory;

        public PlayerViewService(DataFactory dataFactory)
        {
            this.dataFactory = dataFactory;
        }

        public async Task<List<PlayersView>?> GetPlayersView()
        {
            var playersView = await dataFactory.GetPlayersView();

            if (playersView == null)
            {
                // Return null if the data layer fails to retrieve the list
                return null;
            }

            foreach (var player in playersView)
            {
                if (player.GamesPlayed > 0)
                {
                    player.Average = player.GamesWon / (decimal)player.GamesPlayed;
                }
                else
                {
                    player.Average = 0;
                }
            }
            return playersView;
        }
    }
}