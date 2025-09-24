using System.ComponentModel.DataAnnotations;

namespace WellandPoolLeagueMud.Data.ViewModels
{
    public class ScheduleViewModel
    {
        public int ScheduleId { get; set; }

        [Required(ErrorMessage = "Week Number is required")]
        [Range(1, 52, ErrorMessage = "Week Number must be between 1 and 52")]
        public int WeekNumber { get; set; }

        [Required(ErrorMessage = "Home Team is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Home Team is required")]
        public int HomeTeamId { get; set; }

        public string? HomeTeamName { get; set; }

        [Required(ErrorMessage = "Away Team is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Away Team is required")]
        public int AwayTeamId { get; set; }

        public string? AwayTeamName { get; set; }

        [Required(ErrorMessage = "Game Date is required")]
        public DateTime? GameDate { get; set; }

        public int? WinningTeamId { get; set; }

        public string? WinningTeamName { get; set; }

        public bool IsComplete { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        // New property for table number assignment
        [Range(1, 50, ErrorMessage = "Table Number must be between 1 and 50")]
        public int? TableNumber { get; set; }

        public string GameResult => IsComplete ?
            (WinningTeamName ?? "Unknown") : "Not Played";

        public ScheduleViewModel() { }

        public ScheduleViewModel(ScheduleViewModel other)
        {
            this.ScheduleId = other.ScheduleId;
            this.WeekNumber = other.WeekNumber;
            this.HomeTeamId = other.HomeTeamId;
            this.HomeTeamName = other.HomeTeamName;
            this.AwayTeamId = other.AwayTeamId;
            this.AwayTeamName = other.AwayTeamName;
            this.GameDate = other.GameDate;
            this.WinningTeamId = other.WinningTeamId;
            this.WinningTeamName = other.WinningTeamName;
            this.IsComplete = other.IsComplete;
            this.Notes = other.Notes;
            this.TableNumber = other.TableNumber;
        }
    }
}