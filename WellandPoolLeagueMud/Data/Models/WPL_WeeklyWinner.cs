// Data/Models/WPL_WeeklyWinner.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WellandPoolLeagueMud.Data.Models
{
    public class WPL_WeeklyWinner
    {
        [Key]
        public int WeeklyWinnerId { get; set; }
        [Required]
        public int WeekNumber { get; set; }
        [Required]
        public int WinningTeamId { get; set; }

        [ForeignKey("WinningTeamId")]
        public virtual WPL_Team? WinningTeam { get; set; }
    }
}