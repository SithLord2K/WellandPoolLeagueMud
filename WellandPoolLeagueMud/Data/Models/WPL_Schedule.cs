// Data/Models/WPL_Schedule.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WellandPoolLeagueMud.Data.Models
{
    public class WPL_Schedule
    {
        [Key]
        public int ScheduleId { get; set; }
        [Required]
        public int WeekNumber { get; set; }
        [Required]
        public DateTime GameDate { get; set; }
        [Required]
        public int HomeTeamId { get; set; }
        [Required]
        public int AwayTeamId { get; set; }
        public bool Playoffs { get; set; }
        public int? TableNumber { get; set; }
        public DateTime Date { get; set; }

        [ForeignKey("HomeTeamId")]
        public virtual WPL_Team? HomeTeam { get; set; }

        [ForeignKey("AwayTeamId")]
        public virtual WPL_Team? AwayTeam { get; set; }

        public int? WinningTeamId { get; set; }

        public bool Forfeit { get; set; }

        // Optional: Navigation properties to link to WPL_Team
        public WPL_Team? WinningTeam { get; set; }
    }
}