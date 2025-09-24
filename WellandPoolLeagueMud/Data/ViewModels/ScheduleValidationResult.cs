namespace WellandPoolLeagueMud.Data.ViewModels
{
    public class ScheduleValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Warnings { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
}