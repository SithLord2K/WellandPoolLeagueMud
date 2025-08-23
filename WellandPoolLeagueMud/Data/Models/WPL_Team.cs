// Data/Models/WPL_Team.cs
using System.ComponentModel.DataAnnotations;

namespace WellandPoolLeagueMud.Data.Models
{
    public class WPL_Team
    {
        [Key]
        public int TeamId { get; set; }
        [Required]
        public string TeamName { get; set; } = string.Empty;
        public int? CaptainPlayerId { get; set; }

        public virtual ICollection<WPL_Player> Players { get; set; } = new List<WPL_Player>();
    }
}