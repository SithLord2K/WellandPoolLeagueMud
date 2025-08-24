namespace WellandPoolLeagueMud.Data.ViewModels
{
    public class PlayerStandingViewModel
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string? TeamName { get; set; }
        public int GamesPlayed { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public decimal WinPercentage { get; set; }
        public int Rank { get; set; }
    }
}