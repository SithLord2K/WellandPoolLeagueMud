// Data/Models/WPL_Player.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WellandPoolLeagueMud.Data.Models
{
    public class WPL_Player
    {
        [Key]
        public int PlayerId { get; set; }
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        public int GamesPlayed { get; set; }
        public bool IsCaptain { get; set; }
        public int TeamId { get; set; }

        [ForeignKey("TeamId")]
        public virtual WPL_Team? Team { get; set; }
        public virtual ICollection<WPL_PlayerGame> PlayerGames { get; set; } = new List<WPL_PlayerGame>();
    }
}