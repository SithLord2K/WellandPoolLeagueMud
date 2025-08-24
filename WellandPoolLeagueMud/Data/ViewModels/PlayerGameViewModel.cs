using System.ComponentModel.DataAnnotations;

namespace WellandPoolLeagueMud.Data.ViewModels
{
    public class PlayerGameViewModel
    {
        public int PlayerGameId { get; set; }

        [Required(ErrorMessage = "Player is required")]
        public int PlayerId { get; set; }
        public string? PlayerName { get; set; }

        [Required(ErrorMessage = "Team is required")]
        public int TeamId { get; set; }
        public string? TeamName { get; set; }

        [Required(ErrorMessage = "Week Number is required")]
        [Range(1, 52, ErrorMessage = "Week Number must be between 1 and 52")]
        public int WeekNumber { get; set; }

        public bool IsWin { get; set; }

        [Required(ErrorMessage = "Game Date is required")]
        public DateTime GameDate { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
}
