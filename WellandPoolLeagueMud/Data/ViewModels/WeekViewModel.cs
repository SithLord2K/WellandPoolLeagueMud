// Data/ViewModels/WeekViewModel.cs

namespace WellandPoolLeagueMud.Data.ViewModels
{
    public class WeekViewModel
    {
        public int WeekNumber { get; set; }
        public DateTime Date { get; set; } // Added this property
        public int HomeTeamId { get; set; }
        public int AwayTeamId { get; set; }
        public string HomeTeamName { get; set; } = string.Empty;
        public string AwayTeamName { get; set; } = string.Empty;
        public int? WinningTeamId { get; set; }
        public bool IsForfeit { get; set; }
        public bool IsPlayoff { get; set; }
        public int TableNumber { get; set; } // Added this property
    }
}