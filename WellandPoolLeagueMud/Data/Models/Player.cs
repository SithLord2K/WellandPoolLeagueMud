using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WellandPoolLeagueMud.Data.Models
{
    public class Player
    {
        public int PlayerId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? LastName { get; set; }

        public int? TeamId { get; set; }

        [ForeignKey("TeamId")]
        public virtual Team? Team { get; set; }
        public string? Auth0UserId { get; set; }

        public virtual UserProfile? UserProfile { get; set; }

        public virtual ICollection<PlayerGame> PlayerGames { get; set; } = new List<PlayerGame>();
        public virtual ICollection<Team> CaptainedTeams { get; set; } = new List<Team>();
    }
}