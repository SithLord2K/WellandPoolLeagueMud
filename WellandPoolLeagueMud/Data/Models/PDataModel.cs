namespace WellandPoolLeagueMud.Data.Models
{
    public class PDataModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int GamesWon { get; set; }
        public int GamesLost { get; set; }
        public int GamesPlayed { get; set; }
        public decimal Average { get; set; }
        public string? TeamName { get; set; }
    }
}
