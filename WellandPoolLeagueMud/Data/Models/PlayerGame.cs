using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WellandPoolLeagueMud.Data.Models
{
    public class PlayerGame
    {
        public int PlayerGameId { get; set; }
        public int PlayerId { get; set; }
        [ForeignKey("PlayerId")]
        public virtual Player Player { get; set; } = null!;
        public int TeamId { get; set; }
        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; } = null!;
        public int WeekNumber { get; set; }

        public int Wins { get; set; }
        public int Losses { get; set; }

        public DateTime GameDate { get; set; }
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}