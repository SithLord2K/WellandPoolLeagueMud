namespace WellandPoolLeagueMud.Data.Models
{
    public class PlayerData
    {
        public int PlayerId { get; set; }
        public int GamesWon { get; set; }
        public int GamesLost { get; set; }
        public int WeekNumber { get; set; }
        public int GamesPlayed { get; set; }
        public decimal Average { get; set; }
        public int TeamId { get; set; }
    }
}
