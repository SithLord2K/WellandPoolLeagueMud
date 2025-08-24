using System.ComponentModel.DataAnnotations;

namespace WellandPoolLeagueMud.Data.ViewModels
{
    public class TeamViewModel
    {
        public int TeamId { get; set; }

        [Required(ErrorMessage = "Team Name is required")]
        [StringLength(100, ErrorMessage = "Team Name cannot exceed 100 characters")]
        public string TeamName { get; set; } = string.Empty;

        public int? CaptainPlayerId { get; set; }
        public string? CaptainName { get; set; }

        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public int GamesLost { get; set; }
        public decimal WinPercentage => GamesPlayed > 0 ? (decimal)GamesWon / GamesPlayed * 100 : 0;
        public int Points => GamesWon * 2; // Assuming 2 points per win

        public List<PlayerViewModel> Players { get; set; } = new List<PlayerViewModel>();
    }
}