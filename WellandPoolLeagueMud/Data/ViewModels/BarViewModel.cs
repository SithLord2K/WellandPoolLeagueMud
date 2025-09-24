using System.ComponentModel.DataAnnotations;

namespace WellandPoolLeagueMud.Data.ViewModels
{
    public class BarViewModel
    {
        public int BarId { get; set; }

        [Required(ErrorMessage = "Bar Name is required")]
        [StringLength(100, ErrorMessage = "Bar Name cannot exceed 100 characters")]
        public string BarName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Number of Tables is required")]
        [Range(1, 20, ErrorMessage = "Number of Tables must be between 1 and 20")]
        public int NumberOfTables { get; set; }

        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters")]
        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        // Navigation properties
        public List<TeamViewModel> Teams { get; set; } = new();

        // Calculated properties
        public int MaxTeams => NumberOfTables * 2;
        public int CurrentTeamCount => Teams?.Count ?? 0;
        public bool HasAvailableSlots => CurrentTeamCount < MaxTeams;
        public int AvailableSlots => Math.Max(0, MaxTeams - CurrentTeamCount);
    }
}