using WellandPoolLeagueMud.Data.Models;

namespace WellandPoolLeagueMud.Data.ViewModels
{
    public class UserViewModel
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public IEnumerable<string> Roles { get; set; } = new List<string>();
        public bool IsBlocked { get; set; }

        // Player linking properties
        public Player? LinkedPlayer { get; set; }
        public bool HasLinkedPlayer => LinkedPlayer != null;
    }

}