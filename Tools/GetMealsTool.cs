using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Services;
using Models;

namespace Tools;

[McpServerToolType]
public sealed class GetLatestMealsTool
{
    private readonly NightscoutService _nightscoutService;
    private readonly ILogger<GetLatestMealsTool> _logger;

    public GetLatestMealsTool(NightscoutService nightscoutService, ILogger<GetLatestMealsTool> logger)
    {
        _nightscoutService = nightscoutService;
        _logger = logger;
    }

    [McpServerTool, Description("Displays the logged meals (includes carbs amount in grams/\"g\", and meal description)")]
    public async Task<string> GetMeals(int count = 12)
    {
        try
        {
            var treatments = await _nightscoutService.GetTreatmentsAsync("Carbs", count);
            
            if (!treatments.Any())
            {
                return "No meals found.";
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
                "Time | Carbs (g) | Meal description\n" +
                string.Join("\n", group.Treatments.Select(treatment =>
                    $"{NightscoutService.FormatDateTimeString(treatment.CreatedAt):HH:mm} | {treatment.Carbs} | {treatment.Notes?.Replace("name=", "")}"))));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching latest meals");
            return $"Error retrieving meals: {ex.Message}";
        }
    }
}