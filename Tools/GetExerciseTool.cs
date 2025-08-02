using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Services;
using Models;

namespace Tools;

[McpServerToolType]
public sealed class GetLatestExerciseTool
{
    private readonly NightscoutService _nightscoutService;
    private readonly ILogger<GetLatestExerciseTool> _logger;

    public GetLatestExerciseTool(NightscoutService nightscoutService, ILogger<GetLatestExerciseTool> logger)
    {
        _nightscoutService = nightscoutService;
        _logger = logger;
    }

    [McpServerTool, Description("Displays the logged exercise (includes exercise description)")]
    public async Task<string> GetExercise(int count = 12)
    {
        try
        {
            var treatments = await _nightscoutService.GetTreatmentsAsync("Exercise", count);
            
            if (!treatments.Any())
            {
                return "No exercise found.";
            }

            var groupedByDate = treatments.OrderBy(t => t.CreatedAt)
                .GroupBy(t => NightscoutService.FormatDateTimeString(t.CreatedAt).Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Treatments = g.OrderBy(t => t.CreatedAt).ToList()
                });

            var result = string.Join("\n\n", groupedByDate.Select(group =>
                $"Date: {group.Date:yyyy-MM-dd}\n" +
                "Time | Exercise (minutes) | Exercise description\n" +
                string.Join("\n", group.Treatments.Select(treatment =>
                    $"{NightscoutService.FormatDateTimeString(treatment.CreatedAt):HH:mm} | {treatment.Duration} | {treatment.Notes?.Replace("name=", "")}"))));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching latest exercise");
            return $"Error retrieving exercise: {ex.Message}";
        }
    }
}