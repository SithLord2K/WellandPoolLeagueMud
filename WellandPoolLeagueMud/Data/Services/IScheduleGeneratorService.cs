using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WellandPoolLeagueMud.Data.ViewModels;

namespace WellandPoolLeagueMud.Data.Services
{
    public interface IScheduleGeneratorService
    {
        Task<List<ScheduleViewModel>> GenerateSingleRoundRobinScheduleAsync(DateTime startDate, DayOfWeek gameDay);
        Task<List<ScheduleViewModel>> GenerateDoubleRoundRobinScheduleAsync(DateTime startDate, DayOfWeek gameDay);
        Task<List<ScheduleViewModel>> GenerateRandomScheduleAsync(DateTime startDate, int weeksToGenerate, DayOfWeek gameDay, bool ensureBalanced, int? seed = null);
        //Task<List<ScheduleViewModel>> GenerateRandomScheduleAsync(DateTime startDate, int weeksToGenerate, DayOfWeek gameDay, bool ensureBalanced);
        Task<ScheduleValidationResult> ValidateScheduleAsync(List<ScheduleViewModel> scheduleItems);
        Task<int> SaveScheduleBatchAsync(List<ScheduleViewModel> scheduleItems);
    }
}