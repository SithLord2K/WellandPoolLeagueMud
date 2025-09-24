using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WellandPoolLeagueMud.Data.Models
{
    public class Team
    {
        public int TeamId { get; set; }

        [Required]
        [StringLength(100)]
        public string TeamName { get; set; } = string.Empty;

        public int? CaptainPlayerId { get; set; }

        [ForeignKey("CaptainPlayerId")]
        public virtual Player? Captain { get; set; }

        [ForeignKey("Bar")]
        public int? BarId { get; set; }

        public virtual Bar? Bar { get; set; }

        public virtual ICollection<Player> Players { get; set; } = new List<Player>();
        public virtual ICollection<PlayerGame> PlayerGames { get; set; } = new List<PlayerGame>();
        public virtual ICollection<Schedule> HomeGames { get; set; } = new List<Schedule>();
        public virtual ICollection<Schedule> AwayGames { get; set; } = new List<Schedule>();
        public virtual ICollection<Schedule> WonGames { get; set; } = new List<Schedule>();
    }
}