using WellandPoolLeagueMud.Data.ViewModels;

namespace WellandPoolLeagueMud.Data.Services
{
    public interface IScheduleService
    {
        Task<List<ScheduleViewModel>> GetAllSchedulesAsync();
        Task<ScheduleViewModel?> GetScheduleByIdAsync(int id);
        Task<ScheduleViewModel> CreateScheduleAsync(ScheduleViewModel schedule);
        Task<List<ScheduleViewModel>> CreateScheduleBatchAsync(List<ScheduleViewModel> schedules);
        Task<ScheduleViewModel?> UpdateScheduleAsync(ScheduleViewModel schedule);
        Task<bool> DeleteScheduleAsync(int id);
        Task<List<ScheduleViewModel>> GetSchedulesByWeekAsync(int weekNumber);
        Task<List<ScheduleViewModel>> GetSchedulesByTeamAsync(int teamId);
        Task<List<ScheduleViewModel>> GetCompletedSchedulesAsync();
        Task<List<ScheduleViewModel>> GetUpcomingSchedulesAsync();
        Task<bool> ScheduleExistsAsync(int id);
        Task<bool> CompleteScheduleAsync(int scheduleId, int winningTeamId);
    }
}