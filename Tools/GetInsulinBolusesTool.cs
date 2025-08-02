using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Services;
using Models;

namespace Tools;

[McpServerToolType]
public sealed class GetLatestBolusesTool
{
    private readonly NightscoutService _nightscoutService;
    private readonly ILogger<GetLatestBolusesTool> _logger;

    public GetLatestBolusesTool(NightscoutService nightscoutService, ILogger<GetLatestBolusesTool> logger)
    {
        _nightscoutService = nightscoutService;
        _logger = logger;
    }

    [McpServerTool, Description("Displays the latest insulin boluses amount (units or \"u\")")]
    public async Task<string> GetInsulinBoluses(int count = 12)
    {
        try
        {
            var treatments = await _nightscoutService.GetTreatmentsAsync("Bolus", count);
            
            if (!treatments.Any())
            {
                return "No insulin boluses found.";
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
                "Time | Bolus Insulin Amount (u) | Insulin Name\n" +
                string.Join("\n", group.Treatments.Select(treatment =>
                    $"{NightscoutService.FormatDateTimeString(treatment.CreatedAt):HH:mm} | {treatment.Insulin} | {treatment.Notes?.Replace("name=", "")}"))));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching latest insulin boluses");
            return $"Error retrieving insulin boluses: {ex.Message}";
        }
    }
}