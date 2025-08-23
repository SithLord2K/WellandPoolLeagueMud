// Data/ViewModels/PlayerViewModel.cs

namespace WellandPoolLeagueMud.Data.ViewModels
{
    public class PlayerViewModel
    {
        public int PlayerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public int FramesWon { get; set; }
        public int FramesLost { get; set; }
        public int GamesPlayed { get; set; }
        public decimal Average { get; set; }
    }
}