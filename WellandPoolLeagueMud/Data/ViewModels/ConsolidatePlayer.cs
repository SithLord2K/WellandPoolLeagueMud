namespace WellandPoolLeagueMud.Data.Services
{
    public class ConsolidatedPlayer
    {
        public int PlayerId { get; set; }
        public int TeamId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int FramesWon { get; set; }
        public int FramesLost { get; set; }
        public int GamesPlayed => FramesWon + FramesLost;
        public decimal Average => GamesPlayed > 0 ? (decimal)FramesWon / GamesPlayed : 0;
    }
}