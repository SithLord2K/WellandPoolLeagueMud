namespace WellandPoolLeagueMud.Data.ViewModels
{
    public class PlayerSeasonStatsViewModel
    {
        public string Season { get; set; }
        public int GamesPlayed { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public decimal WinPercentage { get; set; }
    }
}