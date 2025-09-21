namespace WellandPoolLeagueMud.ViewModels
{
    public class UserViewModel
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public IEnumerable<string> Roles { get; set; } = new List<string>();
        public bool IsBlocked { get; set; }
    }
}