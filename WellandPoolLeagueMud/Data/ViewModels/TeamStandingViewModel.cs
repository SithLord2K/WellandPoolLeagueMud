namespace WellandPoolLeagueMud.Data.ViewModels
{
    public class TeamStandingViewModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string? CaptainName { get; set; }
        public int GamesPlayed { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Points { get; set; }
        public decimal WinPercentage { get; set; }
        public int Rank { get; set; }
        public string Division { get; set; } = string.Empty;
    }
}