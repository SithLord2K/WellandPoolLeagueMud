using System.ComponentModel.DataAnnotations;

namespace WellandPoolLeagueMud.Data.ViewModels
{
    public class PlayerViewModel
    {
        public int PlayerId { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [StringLength(100, ErrorMessage = "First Name cannot exceed 100 characters")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Last Name cannot exceed 100 characters")]
        public string? LastName { get; set; }

        public int? TeamId { get; set; }
        public string? TeamName { get; set; }

        public string FullName => string.IsNullOrEmpty(LastName) ? FirstName : $"{FirstName} {LastName}";
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public int GamesLost { get; set; }
        public decimal WinPercentage => GamesPlayed > 0 ? (decimal)GamesWon / GamesPlayed * 100 : 0;
    }
}