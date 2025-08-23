// Data/ViewModels/TeamStatsViewModel.cs

namespace WellandPoolLeagueMud.Data.ViewModels
{
    public class TeamStatsViewModel
    {
        public string TeamName { get; set; } = string.Empty;
        public int TotalFramesWon { get; set; }
        public int TotalFramesLost { get; set; }
        public int WeeksWon { get; set; }
        public int WeeksLost { get; set; }
        public int WeeksPlayed { get; set; }
        public int TotalFramesPlayed { get; set; }
        public decimal TotalAverage { get; set; }
        public decimal Points { get; set; }
    }
}