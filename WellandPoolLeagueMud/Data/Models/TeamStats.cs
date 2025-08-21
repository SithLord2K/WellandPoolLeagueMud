namespace WellandPoolLeagueMud.Data.Models
{
    public class TeamStats
    {
        public string? TeamName { get; set; }
        public int TotalGamesWon { get; set; }
        public int TotalGamesLost { get; set; }
        public int TotalGamesPlayed { get; set; }
        public decimal TotalAverage { get; set; }
        public int WeeksWon { get; set; }
        public int WeeksLost { get; set; }
        public int WeeksPlayed { get; set; }
    }
}
