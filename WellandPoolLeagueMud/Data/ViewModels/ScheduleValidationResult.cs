namespace WellandPoolLeagueMud.Data.ViewModels
{
    // Remove this entire class definition:
    public class ScheduleValidationResult
    {
        public bool IsValid => !Errors.Any();
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}