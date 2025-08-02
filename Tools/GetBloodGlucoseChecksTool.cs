using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Services;
using Models;

namespace Tools;

[McpServerToolType]
public sealed class GetLatestBloodGlucoseChecksTool
{
    private readonly NightscoutService _nightscoutService;
    private readonly ILogger<GetLatestBloodGlucoseChecksTool> _logger;

    public GetLatestBloodGlucoseChecksTool(NightscoutService nightscoutService, ILogger<GetLatestBloodGlucoseChecksTool> logger)
    {
        _nightscoutService = nightscoutService;
        _logger = logger;
    }

    [McpServerTool, Description("Displays the finger prick/capillary/glucometer blood glucose checks")]
    public async Task<string> GetBloodGlucoseChecks(int count = 12)
    {
        try
        {
            var treatments = await _nightscoutService.GetTreatmentsAsync("BG Check", count);
            
            if (!treatments.Any())
            {
                return "No Blood Glucose Checks found.";
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
                "Time | Blood Glucose Check\n" +
                string.Join("\n", group.Treatments.Select(treatment =>
                    $"{NightscoutService.FormatDateTimeString(treatment.CreatedAt):HH:mm} | {treatment.Glucose} {treatment.Units}"))));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching latest meals");
            return $"Error retrieving meals: {ex.Message}";
        }
    }
}