namespace WellandPoolLeagueMud.Data.Models
{
    public class Auth0Users
    {
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? UserId { get; set; }
        public string? GivenName { get; set; }
        public string? FamilyName { get; set; }
        public Roles? Role { get; set; }



        public class Roles
        {
            public string[]? roles { get; set; }
        }
    }
}
