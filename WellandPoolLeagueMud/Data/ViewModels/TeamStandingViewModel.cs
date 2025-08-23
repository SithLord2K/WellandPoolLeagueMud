// Data/ViewModels/TeamStandingViewModel.cs

namespace WellandPoolLeagueMud.Data.ViewModels
{
    public class TeamStandingViewModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public int WeeksWon { get; set; }
        public int WeeksLost { get; set; }
        public int TotalFramesWon { get; set; }
        public int TotalFramesLost { get; set; }
        public decimal Points { get; set; }
        public string Division { get; set; } = string.Empty;
        public int Rank { get; set; }
    }
}