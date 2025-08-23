// Data/ViewModels/TeamRoster.cs

using WellandPoolLeagueMud.Data.Models;

namespace WellandPoolLeagueMud.Data.ViewModels
{
    public class TeamRoster
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public List<WPL_Player> Players { get; set; } = new List<WPL_Player>();
    }
}