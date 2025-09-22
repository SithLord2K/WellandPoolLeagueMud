using System.ComponentModel.DataAnnotations;

namespace WellandPoolLeagueMud.Data.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string Auth0UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50)]
        public string? LastName { get; set; }

        [StringLength(300, ErrorMessage = "Bio cannot be longer than 300 characters.")]
        public string? Bio { get; set; }

        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        public string? PhoneNumber { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }
}