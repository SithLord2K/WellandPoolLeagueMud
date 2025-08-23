// Data/ViewModels/TeamStats.cs

namespace WellandPoolLeagueMud.Data.Services
{
    public class TeamStats
    {
        public int TotalFramesWon { get; set; }
        public int TotalFramesLost { get; set; }
        public int WeeksPlayed { get; set; }
        public decimal TotalAverage { get; set; }
    }
}