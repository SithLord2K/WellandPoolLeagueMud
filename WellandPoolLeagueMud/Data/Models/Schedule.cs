using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WellandPoolLeagueMud.Data.Models
{
    public class Schedule
    {
        public int ScheduleId { get; set; }

        public int WeekNumber { get; set; }

        public int HomeTeamId { get; set; }

        [ForeignKey("HomeTeamId")]
        public virtual Team HomeTeam { get; set; } = null!;

        public int AwayTeamId { get; set; }

        [ForeignKey("AwayTeamId")]
        public virtual Team AwayTeam { get; set; } = null!;

        public DateTime GameDate { get; set; }

        public int? WinningTeamId { get; set; }

        [ForeignKey("WinningTeamId")]
        public virtual Team? WinningTeam { get; set; }

        public bool IsComplete { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
        public int? TableNumber { get; set; }
    }
}