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
        private readonly IBarService _barService;
        private readonly ILogger<ScheduleGeneratorService> _logger;

        public ScheduleGeneratorService(ITeamService teamService, IBarService barService, ILogger<ScheduleGeneratorService> logger)
        {
            _teamService = teamService;
            _barService = barService;
            _logger = logger;
        }

        public async Task<List<ScheduleViewModel>> GenerateSingleRoundRobinScheduleAsync(DateTime startDate, DayOfWeek gameDay)
        {
            var teams = await _teamService.GetAllTeamsAsync();
            if (teams.Count < 2) return new List<ScheduleViewModel>();

            var allBars = await _barService.GetActiveBarsAsync();
            var barsDict = allBars.ToDictionary(b => b.BarId);

            var schedule = new List<ScheduleViewModel>();
            var teamVms = teams.ToList();

            if (teamVms.Count % 2 != 0)
            {
                teamVms.Add(new TeamViewModel { TeamId = -1, TeamName = "BYE", BarId = -1 });
            }

            int numTeams = teamVms.Count;
            int numRounds = numTeams - 1;
            var teamNames = teamVms.ToDictionary(t => t.TeamId, t => t.TeamName);
            var teamBars = teamVms.ToDictionary(t => t.TeamId, t => t.BarId);
            var teamIds = teamVms.Select(t => t.TeamId).ToList();

            while (startDate.DayOfWeek != gameDay)
            {
                startDate = startDate.AddDays(1);
            }

            for (int round = 0; round < numRounds; round++)
            {
                var gameDate = startDate.AddDays(round * 7);
                var weeklyHomeBarIds = new HashSet<int>();
                var barTableAssignments = new Dictionary<int, int>();

                for (int i = 0; i < numTeams / 2; i++)
                {
                    int homeTeamId = teamIds[i];
                    int awayTeamId = teamIds[numTeams - 1 - i];

                    if (homeTeamId == -1 || awayTeamId == -1) continue;

                    var homeTeamName = teamNames[homeTeamId];
                    var awayTeamName = teamNames[awayTeamId];
                    var homeBarId = teamBars[homeTeamId];
                    var awayBarId = teamBars[awayTeamId];

                    if (homeBarId.HasValue && weeklyHomeBarIds.Contains(homeBarId.Value))
                    {
                        if (awayBarId.HasValue && !weeklyHomeBarIds.Contains(awayBarId.Value))
                        {
                            (homeTeamId, awayTeamId) = (awayTeamId, homeTeamId);
                            (homeTeamName, awayTeamName) = (awayTeamName, homeTeamName);
                            (homeBarId, awayBarId) = (awayBarId, homeBarId);
                        }
                        else
                        {
                            _logger.LogWarning("Scheduling conflict in round {Round}: Both bars already have home teams", round + 1);
                        }
                    }

                    if (homeBarId.HasValue)
                    {
                        weeklyHomeBarIds.Add(homeBarId.Value);
                    }

                    string notes = null!;

                    if (homeBarId.HasValue && barsDict.TryGetValue(homeBarId.Value, out var barInfo) && barInfo.NumberOfTables > 1)
                    {
                        if (!barTableAssignments.ContainsKey(homeBarId.Value))
                        {
                            barTableAssignments[homeBarId.Value] = 1;
                        }
                        notes = $"Table {barTableAssignments[homeBarId.Value]++}";
                    }

                    schedule.Add(new ScheduleViewModel
                    {
                        WeekNumber = round + 1,
                        GameDate = gameDate,
                        HomeTeamId = homeTeamId,
                        AwayTeamId = awayTeamId,
                        HomeTeamName = homeTeamName,
                        AwayTeamName = awayTeamName,
                        Notes = notes
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

            var teams = await _teamService.GetAllTeamsAsync();
            var teamBars = teams.ToDictionary(t => t.TeamId, t => t.BarId);

            var allBars = await _barService.GetActiveBarsAsync();
            var barsDict = allBars.ToDictionary(b => b.BarId);

            var secondHalf = new List<ScheduleViewModel>();
            int numRounds = firstHalf.Max(s => s.WeekNumber);

            foreach (var game in firstHalf)
            {
                var newHomeTeamName = game.AwayTeamName;
                var newAwayTeamName = game.HomeTeamName;
                var newHomeTeamId = game.AwayTeamId;
                var newAwayTeamId = game.HomeTeamId;
                var newHomeBarId = teamBars.GetValueOrDefault(newHomeTeamId);
                var weekNumber = game.WeekNumber + numRounds;

                if (newHomeBarId.HasValue)
                {
                    var conflictingGame = secondHalf.FirstOrDefault(s =>
                        s.WeekNumber == weekNumber &&
                        teamBars.GetValueOrDefault(s.HomeTeamId) == newHomeBarId);

                    if (conflictingGame != null)
                    {
                        (newHomeTeamId, newAwayTeamId) = (newAwayTeamId, newHomeTeamId);
                        (newHomeTeamName, newAwayTeamName) = (newAwayTeamName, newHomeTeamName);
                        newHomeBarId = teamBars.GetValueOrDefault(newHomeTeamId);
                    }
                }

                string notes = null!;

                if (newHomeBarId.HasValue && barsDict.TryGetValue(newHomeBarId.Value, out var barInfo) && barInfo.NumberOfTables > 1)
                {
                    var weekGames = secondHalf.Where(s => s.WeekNumber == weekNumber).ToList();
                    var sameBarGamesCount = weekGames.Count(g =>
                        teamBars.GetValueOrDefault(g.HomeTeamId) == newHomeBarId);
                    notes = $"Table {sameBarGamesCount + 1}";
                }

                secondHalf.Add(new ScheduleViewModel
                {
                    WeekNumber = weekNumber,
                    GameDate = game.GameDate.HasValue ? game.GameDate.Value.AddDays(numRounds * 7) : null,
                    HomeTeamId = newHomeTeamId,
                    AwayTeamId = newAwayTeamId,
                    HomeTeamName = newHomeTeamName,
                    AwayTeamName = newAwayTeamName,
                    Notes = notes
                });
            }

            return firstHalf.Concat(secondHalf).ToList();
        }

        public async Task<List<ScheduleViewModel>> GenerateRandomScheduleAsync(DateTime startDate, int weeksToGenerate, DayOfWeek gameDay, bool ensureBalanced)
        {
            var teams = await _teamService.GetAllTeamsAsync();
            if (teams.Count < 2) return new List<ScheduleViewModel>();

            var allBars = await _barService.GetActiveBarsAsync();
            var barsDict = allBars.ToDictionary(b => b.BarId);

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
                var weeklyHomeBarIds = new HashSet<int>();
                var barTableAssignments = new Dictionary<int, int>();

                while (availableTeams.Count >= 2)
                {
                    var potentialHomeTeam = availableTeams[0];
                    var potentialAwayTeam = availableTeams[1];

                    if (potentialHomeTeam.BarId.HasValue && weeklyHomeBarIds.Contains(potentialHomeTeam.BarId.Value))
                    {
                        var alternativeHome = availableTeams.Skip(2)
                            .FirstOrDefault(t => !t.BarId.HasValue || !weeklyHomeBarIds.Contains(t.BarId.Value));

                        if (alternativeHome != null)
                        {
                            var index = availableTeams.IndexOf(alternativeHome);
                            availableTeams[0] = alternativeHome;
                            availableTeams[index] = potentialHomeTeam;
                            potentialHomeTeam = alternativeHome;
                        }
                    }

                    var homeTeam = potentialHomeTeam;
                    var awayTeam = potentialAwayTeam;
                    availableTeams.RemoveRange(0, 2);

                    if (homeTeam.BarId.HasValue)
                    {
                        weeklyHomeBarIds.Add(homeTeam.BarId.Value);
                    }

                    string notes = null!;
                    if (homeTeam.BarId.HasValue && barsDict.TryGetValue(homeTeam.BarId.Value, out var barInfo) && barInfo.NumberOfTables > 1)
                    {
                        if (!barTableAssignments.ContainsKey(homeTeam.BarId.Value))
                        {
                            barTableAssignments[homeTeam.BarId.Value] = 1;
                        }
                        notes = $"Table {barTableAssignments[homeTeam.BarId.Value]++}";
                    }

                    schedule.Add(new ScheduleViewModel
                    {
                        WeekNumber = week,
                        GameDate = gameDate,
                        HomeTeamId = homeTeam.TeamId,
                        HomeTeamName = homeTeam.TeamName,
                        AwayTeamId = awayTeam.TeamId,
                        AwayTeamName = awayTeam.TeamName,
                        Notes = notes
                    });
                }
            }
            return schedule;
        }

        public async Task<ScheduleValidationResult> ValidateScheduleAsync(List<ScheduleViewModel> scheduleItems)
        {
            var result = new ScheduleValidationResult();
            if (scheduleItems == null || !scheduleItems.Any())
            {
                result.Errors.Add("Schedule is empty.");
                return result;
            }

            var teams = await _teamService.GetAllTeamsAsync();
            var teamBars = teams.ToDictionary(t => t.TeamId, t => t.BarId);

            var allBars = await _barService.GetActiveBarsAsync();
            var barsDict = allBars.ToDictionary(b => b.BarId);

            var weeklyBarHomeTeams = scheduleItems
                .GroupBy(s => s.WeekNumber)
                .ToList();

            foreach (var weekGroup in weeklyBarHomeTeams)
            {
                var homeTeamsByBar = weekGroup
                    .GroupBy(g => teamBars.GetValueOrDefault(g.HomeTeamId, -1))
                    .Where(barGroup => barGroup.Key != -1)
                    .ToList();

                foreach (var barGroup in homeTeamsByBar)
                {
                    if (barGroup.Count() > 1)
                    {
                        var barTeamNames = barGroup.Select(g => g.HomeTeamName);
                        result.Errors.Add($"Week {weekGroup.Key}: Multiple home teams from same bar - {string.Join(", ", barTeamNames)}");
                    }
                }
            }

            var gamesMissingRequiredTableAssignment = scheduleItems.Where(s =>
            {
                var homeBarId = teamBars.GetValueOrDefault(s.HomeTeamId);
                if (!homeBarId.HasValue) return false;

                if (barsDict.TryGetValue(homeBarId.Value, out var barInfo) && barInfo.NumberOfTables > 1)
                {
                    return string.IsNullOrEmpty(s.Notes) || !s.Notes.StartsWith("Table ");
                }
                return false;
            }).ToList();

            if (gamesMissingRequiredTableAssignment.Any())
            {
                result.Errors.Add($"Found {gamesMissingRequiredTableAssignment.Count} games that require a table assignment but don't have one.");
            }

            return result;
        }

        public Task<int> SaveScheduleBatchAsync(List<ScheduleViewModel> scheduleItems)
        {
            _logger.LogInformation("Saving a batch of {Count} schedule items.", scheduleItems.Count);
            return Task.FromResult(scheduleItems.Count);
        }
    }
}