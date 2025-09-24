namespace WellandPoolLeagueMud.Data.ViewModels
{
    public class ScheduleGenerationResult
    {
        public List<ScheduleViewModel> Schedules { get; set; } = new();
        public int TotalWeeks { get; set; }
        public int GamesPerWeek { get; set; }
        public bool HasByeWeeks { get; set; }
        public List<string> Warnings { get; set; } = new();
    }
}