// Data/Models/WPL_PlayerGame.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WellandPoolLeagueMud.Data.Models
{
    public class WPL_PlayerGame
    {
        [Key]
        public int PlayerGameId { get; set; }
        [Required]
        public int PlayerId { get; set; }
        [Required]
        public int ScheduleId { get; set; }
        [Required]
        public int FramesWon { get; set; }
        [Required]
        public int FramesLost { get; set; }
        public int WeekNumber { get; set; }

        [ForeignKey("PlayerId")]
        public virtual WPL_Player? Player { get; set; }

        [ForeignKey("ScheduleId")]
        public virtual WPL_Schedule? Schedule { get; set; }
    }
}