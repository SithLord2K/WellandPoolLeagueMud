using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WellandPoolLeagueMud.Data.ViewModels;

namespace WellandPoolLeagueMud.Data.Services
{
    public class ScheduleGeneratorService : IScheduleGeneratorService
    {
        private readonly ITeamService _teamService;
        private readonly ILogger<ScheduleGeneratorService> _logger;

        public ScheduleGeneratorService(ITeamService teamService, ILogger<ScheduleGeneratorService> logger)
        {
            _teamService = teamService;
            _logger = logger;
        }

        public async Task<List<ScheduleViewModel>> GenerateSingleRoundRobinScheduleAsync(DateTime startDate, DayOfWeek gameDay)
        {
            var teams = await _teamService.GetAllTeamsAsync();
            if (teams.Count < 2) return new List<ScheduleViewModel>();

            var schedule = new List<ScheduleViewModel>();
            var teamVms = teams.ToList();

            if (teamVms.Count % 2 != 0)
            {
                teamVms.Add(new TeamViewModel { TeamId = -1, TeamName = "BYE" });
            }

            int numTeams = teamVms.Count;
            int numRounds = numTeams - 1;
            var teamNames = teamVms.ToDictionary(t => t.TeamId, t => t.TeamName);
            var teamIds = teamVms.Select(t => t.TeamId).ToList();

            while (startDate.DayOfWeek != gameDay)
            {
                startDate = startDate.AddDays(1);
            }

            for (int round = 0; round < numRounds; round++)
            {
                var gameDate = startDate.AddDays(round * 7);
                for (int i = 0; i < numTeams / 2; i++)
                {
                    int homeTeamId = teamIds[i];
                    int awayTeamId = teamIds[numTeams - 1 - i];

                    if (homeTeamId == -1 || awayTeamId == -1) continue;

                    schedule.Add(new ScheduleViewModel
                    {
                        WeekNumber = round + 1,
                        GameDate = gameDate,
                        HomeTeamId = homeTeamId,
                        AwayTeamId = awayTeamId,
                        HomeTeamName = teamNames[homeTeamId],
                        AwayTeamName = teamNames[awayTeamId]
                    });
                }

                var lastTeamId = teamIds.Last();
                teamIds.RemoveAt(teamIds.Count - 1);
                teamIds.Insert(1, lastTeamId);
            }
            return schedule;
        }

        public async Task<List<ScheduleViewModel>> GenerateDoubleRoundRobinScheduleAsync(DateTime startDate, DayOfWeek gameDay)
        {
            var firstHalf = await GenerateSingleRoundRobinScheduleAsync(startDate, gameDay);
            if (!firstHalf.Any()) return firstHalf;

            var secondHalf = new List<ScheduleViewModel>();
            int numRounds = firstHalf.Max(s => s.WeekNumber);

            foreach (var game in firstHalf)
            {
                secondHalf.Add(new ScheduleViewModel
                {
                    WeekNumber = game.WeekNumber + numRounds,
                    GameDate = game.GameDate.HasValue ? game.GameDate.Value.AddDays(numRounds * 7) : null,
                    HomeTeamId = game.AwayTeamId,
                    AwayTeamId = game.HomeTeamId,
                    HomeTeamName = game.AwayTeamName,
                    AwayTeamName = game.HomeTeamName
                });
            }

            return firstHalf.Concat(secondHalf).ToList();
        }

        public async Task<List<ScheduleViewModel>> GenerateRandomScheduleAsync(DateTime startDate, int weeksToGenerate, DayOfWeek gameDay, bool ensureBalanced)
        {
            var teams = await _teamService.GetAllTeamsAsync();
            if (teams.Count < 2) return new List<ScheduleViewModel>();

            var schedule = new List<ScheduleViewModel>();
            var random = new Random();

            while (startDate.DayOfWeek != gameDay)
            {
                startDate = startDate.AddDays(1);
            }

            for (int week = 1; week <= weeksToGenerate; week++)
            {
                var gameDate = startDate.AddDays((week - 1) * 7);
                var availableTeams = teams.OrderBy(t => random.Next()).ToList();

                while (availableTeams.Count >= 2)
                {
                    var homeTeam = availableTeams[0];
                    var awayTeam = availableTeams[1];
                    availableTeams.RemoveRange(0, 2);

                    schedule.Add(new ScheduleViewModel
                    {
                        WeekNumber = week,
                        GameDate = gameDate,
                        HomeTeamId = homeTeam.TeamId,
                        HomeTeamName = homeTeam.TeamName,
                        AwayTeamId = awayTeam.TeamId,
                        AwayTeamName = awayTeam.TeamName,
                    });
                }
            }
            return schedule;
        }

        public Task<ScheduleValidationResult> ValidateScheduleAsync(List<ScheduleViewModel> scheduleItems)
        {
            var result = new ScheduleValidationResult();
            if (scheduleItems == null || !scheduleItems.Any())
            {
                result.Errors.Add("Schedule is empty.");
                return Task.FromResult(result);
            }
            return Task.FromResult(result);
        }

        public Task<int> SaveScheduleBatchAsync(List<ScheduleViewModel> scheduleItems)
        {
            _logger.LogInformation("Saving a batch of {Count} schedule items.", scheduleItems.Count);
            return Task.FromResult(scheduleItems.Count);
        }
    }
}