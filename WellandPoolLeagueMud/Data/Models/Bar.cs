using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WellandPoolLeagueMud.Data.Models
{
    [Table("WPLMud_Bars")]
    public class Bar
    {
        [Key]
        public int BarId { get; set; }

        [Required]
        [StringLength(100)]
        public string BarName { get; set; } = string.Empty;

        [Required]
        [Range(1, 20)]
        public int NumberOfTables { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
    }
}