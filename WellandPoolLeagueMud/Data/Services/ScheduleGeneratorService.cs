using WellandPoolLeagueMud.Data.ViewModels;

namespace WellandPoolLeagueMud.Data.Services
{
    public class ScheduleGeneratorService : IScheduleGeneratorService
    {
        private readonly ITeamService _teamService;
        private readonly IBarService _barService;
        private readonly ILogger<ScheduleGeneratorService> _logger;
        private readonly IScheduleService _scheduleService;
        private const int HomeAwayBalanceTolerance = 2;
        private const int MaxConsecutiveHomeGames = 3;

        // Cache for performance optimization
        private Dictionary<int, BarViewModel> _barsCache = new();
        private Dictionary<int, TeamViewModel> _teamsCache = new();

        public ScheduleGeneratorService(ITeamService teamService, IBarService barService, ILogger<ScheduleGeneratorService> logger, IScheduleService scheduleService)
        {
            _teamService = teamService;
            _barService = barService;
            _logger = logger;
            _scheduleService = scheduleService;
        }

        public async Task<List<ScheduleViewModel>> GenerateSingleRoundRobinScheduleAsync(DateTime startDate, DayOfWeek gameDay)
        {
            return await GenerateScheduleAsync(startDate, gameDay, ScheduleType.SingleRoundRobin, 0, true);
        }

        public async Task<List<ScheduleViewModel>> GenerateDoubleRoundRobinScheduleAsync(DateTime startDate, DayOfWeek gameDay)
        {
            return await GenerateScheduleAsync(startDate, gameDay, ScheduleType.DoubleRoundRobin, 0, true);
        }

        public async Task<List<ScheduleViewModel>> GenerateRandomScheduleAsync(DateTime startDate, int weeksToGenerate, DayOfWeek gameDay, bool ensureBalanced, int? seed = null)
        {
            return await GenerateScheduleAsync(startDate, gameDay, ScheduleType.Custom, weeksToGenerate, ensureBalanced, seed);
        }

        public async Task<ScheduleValidationResult> ValidateConstraintsBeforeGeneration()
        {
            var result = new ScheduleValidationResult();
            var teams = await _teamService.GetAllTeamsAsync();
            var bars = await _barService.GetActiveBarsAsync();

            if (teams.Count < 2)
            {
                result.Errors.Add("At least 2 teams required for schedule generation");
                return result;
            }

            var totalCapacity = bars.Sum(b => b.NumberOfTables);
            var gamesPerWeek = (teams.Count + 1) / 2; // Account for BYE week
            var singleTableBars = bars.Where(b => b.NumberOfTables == 1).ToList();

            if (gamesPerWeek > totalCapacity)
            {
                result.Errors.Add($"Insufficient bar capacity: Need {gamesPerWeek} games/week, only {totalCapacity} tables available");
            }

            if (singleTableBars.Count > gamesPerWeek)
            {
                result.Errors.Add("Impossible schedule: More single-table bars than weekly games possible");
            }

            // Check for teams concentrated at single bars
            var teamsPerBar = teams.Where(t => t.BarId.HasValue)
                .GroupBy(t => t.BarId!.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var barGroup in teamsPerBar)
            {
                var bar = bars.FirstOrDefault(b => b.BarId == barGroup.Key);
                if (bar?.NumberOfTables == 1 && barGroup.Value > 2)
                {
                    result.Warnings.Add($"Bar '{bar.BarName}' has {barGroup.Value} teams but only 1 table - may cause scheduling conflicts");
                }
            }

            return result;
        }

        private async Task<List<ScheduleViewModel>> GenerateScheduleAsync(DateTime startDate, DayOfWeek gameDay, ScheduleType type, int weeksToGenerate, bool ensureBalanced, int? seed = null)
        {
            var constraintCheck = await ValidateConstraintsBeforeGeneration();
            if (!constraintCheck.IsValid)
            {
                _logger.LogError("Cannot generate schedule due to constraint violations: {Errors}", string.Join(", ", constraintCheck.Errors));
                return new List<ScheduleViewModel>();
            }

            var teams = (await _teamService.GetAllTeamsAsync()).ToList();
            var allBars = (await _barService.GetActiveBarsAsync()).ToList();

            // Cache for performance
            _barsCache = allBars.ToDictionary(b => b.BarId);
            _teamsCache = teams.ToDictionary(t => t.TeamId);

            var teamsWithBye = new List<TeamViewModel>(teams);
            if (teamsWithBye.Count % 2 != 0) teamsWithBye.Add(new TeamViewModel { TeamId = -1, TeamName = "BYE" });

            int numTeams = teamsWithBye.Count;
            int numRoundsInHalf = numTeams - 1;
            int totalRounds = type == ScheduleType.DoubleRoundRobin ? numRoundsInHalf * 2 : (type == ScheduleType.SingleRoundRobin ? numRoundsInHalf : weeksToGenerate);

            var schedule = new List<ScheduleViewModel>();
            var teamIds = teamsWithBye.Select(t => t.TeamId).ToList();
            var homeAwayCounts = teamsWithBye.ToDictionary(t => t.TeamId, t => (Home: 0, Away: 0));
            var consecutiveHomeTracker = teamsWithBye.ToDictionary(t => t.TeamId, t => 0);
            var lastHomeTeamBySingleTableBar = new Dictionary<int, int>();
            var firstHalfMatchups = new Dictionary<int, List<(int, int)>>();

            // For deterministic random scheduling
            var random = seed.HasValue ? new Random(seed.Value) : new Random();

            while (startDate.DayOfWeek != gameDay) startDate = startDate.AddDays(1);

            _logger.LogDebug("Generating {Type} schedule for {TeamCount} teams starting {Date}", type, teams.Count, startDate);

            for (int round = 0; round < totalRounds; round++)
            {
                var gameDate = startDate.AddDays(round * 7);
                var weeklyGames = new List<ScheduleViewModel>();
                var weeklyBarCount = new Dictionary<int, int>();

                List<(int, int)> pairings;
                bool isSecondHalf = type == ScheduleType.DoubleRoundRobin && round >= numRoundsInHalf;

                if (isSecondHalf)
                {
                    pairings = firstHalfMatchups[round - numRoundsInHalf].Select(p => (p.Item2, p.Item1)).ToList();
                }
                else
                {
                    pairings = type == ScheduleType.Custom ?
                        GetRandomPairings(teamIds, random) :
                        GetPairingsForRound(teamIds);

                    if (type != ScheduleType.Custom) firstHalfMatchups[round] = pairings;
                }

                foreach (var pair in pairings)
                {
                    if (pair.Item1 == -1 || pair.Item2 == -1) continue;

                    var team1 = _teamsCache[pair.Item1];
                    var team2 = _teamsCache[pair.Item2];

                    var homeTeam = team1;
                    var awayTeam = team2;

                    if (ShouldSwap(homeTeam, awayTeam, weeklyBarCount, homeAwayCounts, lastHomeTeamBySingleTableBar, consecutiveHomeTracker, ensureBalanced))
                    {
                        (homeTeam, awayTeam) = (awayTeam, homeTeam);
                    }

                    weeklyGames.Add(new ScheduleViewModel
                    {
                        WeekNumber = round + 1,
                        GameDate = gameDate,
                        HomeTeamId = homeTeam.TeamId,
                        AwayTeamId = awayTeam.TeamId,
                        HomeTeamName = homeTeam.TeamName,
                        AwayTeamName = awayTeam.TeamName
                    });

                    if (homeTeam.BarId.HasValue)
                    {
                        weeklyBarCount[homeTeam.BarId.Value] = weeklyBarCount.GetValueOrDefault(homeTeam.BarId.Value, 0) + 1;
                    }
                }

                FinalReviewAndFix(weeklyGames, allBars, lastHomeTeamBySingleTableBar, consecutiveHomeTracker, round + 1);

                var tableAssignmentTracker = new Dictionary<int, int>();
                foreach (var game in weeklyGames)
                {
                    var homeTeam = _teamsCache[game.HomeTeamId];
                    homeAwayCounts[game.HomeTeamId] = (homeAwayCounts[game.HomeTeamId].Home + 1, homeAwayCounts[game.HomeTeamId].Away);
                    homeAwayCounts[game.AwayTeamId] = (homeAwayCounts[game.AwayTeamId].Home, homeAwayCounts[game.AwayTeamId].Away + 1);
                    consecutiveHomeTracker[game.HomeTeamId]++;
                    consecutiveHomeTracker[game.AwayTeamId] = 0;

                    if (homeTeam.BarId.HasValue)
                    {
                        var barId = homeTeam.BarId.Value;
                        game.TableNumber = GetTableNumberAssignment(barId, tableAssignmentTracker);

                        var bar = _barsCache[barId];
                        if (bar.NumberOfTables == 1)
                            lastHomeTeamBySingleTableBar[bar.BarId] = homeTeam.TeamId;
                    }
                }
                schedule.AddRange(weeklyGames);

                if (!isSecondHalf && type != ScheduleType.Custom)
                {
                    var lastTeamId = teamIds.Last();
                    teamIds.RemoveAt(teamIds.Count - 1);
                    teamIds.Insert(1, lastTeamId);
                }
            }

            _logger.LogInformation("Generated {GameCount} games across {WeekCount} weeks", schedule.Count, totalRounds);
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
            var teamsDict = teams.ToDictionary(t => t.TeamId);
            var allBars = await _barService.GetActiveBarsAsync();
            var barsDict = allBars.ToDictionary(b => b.BarId);
            var singleTableBars = allBars.Where(b => b.NumberOfTables == 1).ToDictionary(b => b.BarId);
            var homeAwayCounts = teams.ToDictionary(t => t.TeamId, t => (Home: 0, Away: 0));
            var consecutiveHomeTracker = teams.ToDictionary(t => t.TeamId, t => 0);
            var lastHomeTeamBySingleTableBar = new Dictionary<int, int>();
            var weeklyGames = scheduleItems.GroupBy(s => s.WeekNumber).OrderBy(g => g.Key);

            foreach (var weekGroup in weeklyGames)
            {
                var teamsPlayingThisWeek = new HashSet<int>();
                var weeklyBarHomeTeamCount = new Dictionary<int, int>();

                foreach (var game in weekGroup.Where(g => g.HomeTeamId != -1 && g.AwayTeamId != -1)) // Filter BYE games
                {
                    if (!teamsPlayingThisWeek.Add(game.HomeTeamId))
                        result.Errors.Add($"Week {weekGroup.Key}: Team '{game.HomeTeamName}' plays more than once.");
                    if (!teamsPlayingThisWeek.Add(game.AwayTeamId))
                        result.Errors.Add($"Week {weekGroup.Key}: Team '{game.AwayTeamName}' plays more than once.");

                    homeAwayCounts[game.HomeTeamId] = (homeAwayCounts[game.HomeTeamId].Home + 1, homeAwayCounts[game.HomeTeamId].Away);
                    homeAwayCounts[game.AwayTeamId] = (homeAwayCounts[game.AwayTeamId].Home, homeAwayCounts[game.AwayTeamId].Away + 1);
                    consecutiveHomeTracker[game.HomeTeamId]++;
                    consecutiveHomeTracker[game.AwayTeamId] = 0;

                    if (consecutiveHomeTracker[game.HomeTeamId] > MaxConsecutiveHomeGames)
                        result.Errors.Add($"Week {weekGroup.Key}: Team '{game.HomeTeamName}' has more than {MaxConsecutiveHomeGames} consecutive home games.");

                    var homeTeam = teamsDict.GetValueOrDefault(game.HomeTeamId);
                    if (homeTeam?.BarId.HasValue == true)
                    {
                        var barId = homeTeam.BarId.Value;
                        weeklyBarHomeTeamCount[barId] = weeklyBarHomeTeamCount.GetValueOrDefault(barId, 0) + 1;

                        if (singleTableBars.ContainsKey(barId))
                        {
                            if (lastHomeTeamBySingleTableBar.TryGetValue(barId, out var lastHomeTeamId) && lastHomeTeamId == game.HomeTeamId)
                                result.Errors.Add($"Week {weekGroup.Key}: Team '{game.HomeTeamName}' is home at '{singleTableBars[barId].BarName}' for a second consecutive week.");
                            lastHomeTeamBySingleTableBar[barId] = game.HomeTeamId;
                        }
                    }
                }

                foreach (var barCount in weeklyBarHomeTeamCount)
                {
                    if (barsDict.TryGetValue(barCount.Key, out var barInfo) && barCount.Value > barInfo.NumberOfTables)
                        result.Errors.Add($"Week {weekGroup.Key}: Bar '{barInfo.BarName}' is over capacity ({barCount.Value} games / {barInfo.NumberOfTables} tables).");
                }

                var homeBarsUsedThisWeek = weeklyBarHomeTeamCount.Keys;
                foreach (var singleBar in singleTableBars)
                {
                    if (!homeBarsUsedThisWeek.Contains(singleBar.Key))
                        result.Errors.Add($"Week {weekGroup.Key}: Single-table bar '{singleBar.Value.BarName}' has no home game.");
                }
            }

            // Enhanced home/away balance checking
            foreach (var count in homeAwayCounts.Where(kvp => kvp.Key != -1))
            {
                var imbalance = Math.Abs(count.Value.Home - count.Value.Away);
                if (imbalance > HomeAwayBalanceTolerance)
                {
                    var teamName = teamsDict[count.Key].TeamName;
                    var severity = imbalance > HomeAwayBalanceTolerance + 2 ? "major" : "minor";
                    result.Warnings.Add($"Team '{teamName}' has {severity} schedule imbalance: {count.Value.Home} Home, {count.Value.Away} Away.");
                }
            }

            return result;
        }

        public async Task<int> SaveScheduleBatchAsync(List<ScheduleViewModel> scheduleItems)
        {
            if (scheduleItems == null || !scheduleItems.Any())
            {
                _logger.LogWarning("SaveScheduleBatchAsync called with empty schedule list");
                return 0;
            }

            _logger.LogInformation("Starting to save batch of {Count} schedule items", scheduleItems.Count);

            try
            {
                // Ensure all ScheduleIds are 0 for new entities
                foreach (var schedule in scheduleItems)
                {
                    schedule.ScheduleId = 0; // Reset to 0 to ensure EF treats them as new entities

                    if (schedule.HomeTeamId <= 0 || schedule.AwayTeamId <= 0)
                    {
                        _logger.LogError("Invalid schedule found: HomeTeamId={HomeId}, AwayTeamId={AwayId}", schedule.HomeTeamId, schedule.AwayTeamId);
                        throw new InvalidOperationException($"Invalid schedule data: HomeTeamId={schedule.HomeTeamId}, AwayTeamId={schedule.AwayTeamId}");
                    }

                    if (!schedule.GameDate.HasValue)
                    {
                        _logger.LogError("Schedule missing GameDate for Week {Week}", schedule.WeekNumber);
                        throw new InvalidOperationException($"Schedule for Week {schedule.WeekNumber} is missing GameDate");
                    }
                }

                // Use the proper batch save method from IScheduleService
                var savedSchedules = await _scheduleService.CreateScheduleBatchAsync(scheduleItems);

                _logger.LogInformation("Successfully saved {Count} schedule items to database", savedSchedules.Count);
                return savedSchedules.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving schedule batch of {Count} items", scheduleItems.Count);
                throw;
            }
        }

        #region Helper Methods

        private void FinalReviewAndFix(List<ScheduleViewModel> weeklyGames, List<BarViewModel> allBars, Dictionary<int, int> lastHome, Dictionary<int, int> consecutiveHome, int weekNumber)
        {
            var singleTableBars = allBars.Where(b => b.NumberOfTables == 1).ToList();
            var weeklyBarCount = new Dictionary<int, int>();

            foreach (var game in weeklyGames)
            {
                var homeTeam = _teamsCache[game.HomeTeamId];
                if (homeTeam.BarId.HasValue)
                    weeklyBarCount[homeTeam.BarId.Value] = weeklyBarCount.GetValueOrDefault(homeTeam.BarId.Value, 0) + 1;
            }

            var homeBarsUsed = new HashSet<int>(weeklyBarCount.Keys.Where(k => weeklyBarCount[k] > 0));
            var swapsMade = false;

            foreach (var missedBar in singleTableBars.Where(b => !homeBarsUsed.Contains(b.BarId)))
            {
                var teamToMakeHome = _teamsCache.Values.FirstOrDefault(t => t.BarId == missedBar.BarId);
                if (teamToMakeHome == null) continue;

                var gameToSwap = weeklyGames.FirstOrDefault(g => g.AwayTeamId == teamToMakeHome.TeamId);
                if (gameToSwap != null && consecutiveHome.GetValueOrDefault(teamToMakeHome.TeamId, 0) < MaxConsecutiveHomeGames)
                {
                    PerformSwap(gameToSwap, weeklyBarCount);
                    swapsMade = true;
                }
            }

            if (swapsMade)
            {
                _logger.LogDebug("Made swaps in week {Week} to ensure single-table bar coverage", weekNumber);
            }

            // Fix over-capacity bars
            int attempts = 0;
            do
            {
                swapsMade = false;
                var overCapacityBars = allBars.Where(b => weeklyBarCount.GetValueOrDefault(b.BarId, 0) > b.NumberOfTables).ToList();

                foreach (var bar in overCapacityBars)
                {
                    var excessGame = weeklyGames.FirstOrDefault(g =>
                        _teamsCache[g.HomeTeamId].BarId == bar.BarId &&
                        CanBeHome(_teamsCache[g.AwayTeamId], weeklyBarCount, lastHome, consecutiveHome));

                    if (excessGame != null)
                    {
                        PerformSwap(excessGame, weeklyBarCount);
                        swapsMade = true;
                    }
                }
                attempts++;
            } while (swapsMade && attempts < 5);
        }

        private void PerformSwap(ScheduleViewModel game, Dictionary<int, int> weeklyBarCount)
        {
            var originalHomeTeam = _teamsCache[game.HomeTeamId];
            var newHomeTeam = _teamsCache[game.AwayTeamId];

            if (originalHomeTeam.BarId.HasValue) weeklyBarCount[originalHomeTeam.BarId.Value]--;
            if (newHomeTeam.BarId.HasValue) weeklyBarCount[newHomeTeam.BarId.Value] = weeklyBarCount.GetValueOrDefault(newHomeTeam.BarId.Value, 0) + 1;

            (game.HomeTeamId, game.AwayTeamId) = (game.AwayTeamId, game.HomeTeamId);
            (game.HomeTeamName, game.AwayTeamName) = (game.AwayTeamName, game.HomeTeamName);
        }

        private bool ShouldSwap(TeamViewModel team1, TeamViewModel team2, Dictionary<int, int> weeklyBarCount, Dictionary<int, (int Home, int Away)> counts, Dictionary<int, int> lastHome, Dictionary<int, int> consecutiveHome, bool balance)
        {
            var canTeam1BeHome = CanBeHome(team1, weeklyBarCount, lastHome, consecutiveHome);
            var canTeam2BeHome = CanBeHome(team2, weeklyBarCount, lastHome, consecutiveHome);

            if (canTeam1BeHome && !canTeam2BeHome) return false;
            if (!canTeam1BeHome && canTeam2BeHome) return true;

            // Enhanced balance logic
            if (canTeam1BeHome && canTeam2BeHome && balance)
            {
                var team1HomeCount = counts[team1.TeamId].Home;
                var team2HomeCount = counts[team2.TeamId].Home;
                var deficit = team2HomeCount - team1HomeCount;

                // Prioritize team with fewer home games
                if (Math.Abs(deficit) > 1)
                {
                    return deficit > 0; // Give home to team2 if they have fewer
                }

                // Consider consecutive games as tiebreaker
                var team1Consecutive = consecutiveHome.GetValueOrDefault(team1.TeamId, 0);
                var team2Consecutive = consecutiveHome.GetValueOrDefault(team2.TeamId, 0);

                if (team1Consecutive > team2Consecutive) return true;
            }

            return !canTeam1BeHome;
        }

        private bool CanBeHome(TeamViewModel team, Dictionary<int, int> weeklyBarCount, Dictionary<int, int> lastHome, Dictionary<int, int> consecutiveHome)
        {
            if (team.TeamId == -1 || !team.BarId.HasValue) return false;

            var barId = team.BarId.Value;
            if (!_barsCache.TryGetValue(barId, out var bar)) return false;

            if (weeklyBarCount.GetValueOrDefault(barId, 0) >= bar.NumberOfTables) return false;
            if (bar.NumberOfTables == 1 && lastHome.GetValueOrDefault(barId, 0) == team.TeamId) return false;
            if (consecutiveHome.GetValueOrDefault(team.TeamId, 0) >= MaxConsecutiveHomeGames) return false;

            return true;
        }

        private int? GetTableNumberAssignment(int barId, Dictionary<int, int> tableTracker)
        {
            if (_barsCache.TryGetValue(barId, out var bar) && bar.NumberOfTables > 1)
            {
                int gamesAlreadyAtBar = tableTracker.GetValueOrDefault(barId, 0);
                int assignedTable = (gamesAlreadyAtBar % bar.NumberOfTables) + 1;
                tableTracker[barId] = gamesAlreadyAtBar + 1;
                return assignedTable;
            }
            return null;
        }

        private List<(int, int)> GetPairingsForRound(List<int> teamIds)
        {
            var pairings = new List<(int, int)>();
            int numTeams = teamIds.Count;
            for (int i = 0; i < numTeams / 2; i++)
            {
                int team1 = teamIds[i];
                int team2 = teamIds[numTeams - 1 - i];
                pairings.Add((team1, team2));
            }
            return pairings;
        }

        private List<(int, int)> GetRandomPairings(List<int> teamIds, Random random)
        {
            var availableTeams = teamIds.Where(id => id != -1).OrderBy(x => random.Next()).ToList();
            var pairings = new List<(int, int)>();

            for (int i = 0; i < availableTeams.Count - 1; i += 2)
            {
                pairings.Add((availableTeams[i], availableTeams[i + 1]));
            }

            return pairings;
        }

        #endregion
    }

    internal enum ScheduleType { SingleRoundRobin, DoubleRoundRobin, Custom }
}