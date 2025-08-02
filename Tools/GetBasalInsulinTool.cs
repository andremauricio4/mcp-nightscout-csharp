using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Services;
using Models;

namespace Tools;

[McpServerToolType]
public sealed class GetLatestBasalTool
{
    private readonly NightscoutService _nightscoutService;
    private readonly ILogger<GetLatestBasalTool> _logger;

    public GetLatestBasalTool(NightscoutService nightscoutService, ILogger<GetLatestBasalTool> logger)
    {
        _nightscoutService = nightscoutService;
        _logger = logger;
    }

    [McpServerTool, Description("Displays the latest Basal Insulin administrations (amount in units or \"u\")")]
    public async Task<string> GetBasalInsulin(int count = 12)
    {
        try
        {
            var treatments = await _nightscoutService.GetTreatmentsAsync("Temp Basal", count);
            
            if (!treatments.Any())
            {
                return "No Basal Insulin found.";
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
                "Time | Basal Insulin Amount (u) | Insulin Name\n" +
                string.Join("\n", group.Treatments.Select(treatment =>
                    $"{NightscoutService.FormatDateTimeString(treatment.CreatedAt):HH:mm} | {treatment.Absolute} | {treatment.Notes?.Replace("name=", "")}"))));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching latest Basal Insulin");
            return $"Error retrieving Basal Insulin: {ex.Message}";
        }
    }
}