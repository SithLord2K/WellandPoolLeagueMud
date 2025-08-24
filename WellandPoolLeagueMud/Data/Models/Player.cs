using System.ComponentModel.DataAnnotations;

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

        [StringLength(15)]
        public string? Phone { get; set; }

        public virtual ICollection<PlayerGame> PlayerGames { get; set; } = new List<PlayerGame>();
        public virtual ICollection<Team> CaptainedTeams { get; set; } = new List<Team>();
    }
}