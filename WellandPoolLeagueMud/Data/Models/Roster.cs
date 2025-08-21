namespace WellandPoolLeagueMud.Data.Models
{
    public class Roster
    {
        public int TeamId { get; set; }
        public string? TeamName { get; set; }
        public int Captain_Player_Id { get; set; }
        public int Player_Id { get; set; }
        public string? Player_Name { get; set; }
        public bool IsCaptain { get; set; }
    }
}
