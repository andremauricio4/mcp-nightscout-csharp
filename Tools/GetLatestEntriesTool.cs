using ModelContextProtocol.Server;
using System.ComponentModel;
using Services;
using Models;

namespace Tools;

[McpServerToolType]
public sealed class GetLatestEntriesTool
{
    private readonly NightscoutService _nightscoutService;
    private readonly ILogger<GetLatestEntriesTool> _logger;

    public GetLatestEntriesTool(NightscoutService nightscoutService, ILogger<GetLatestEntriesTool> logger)
    {
        _nightscoutService = nightscoutService;
        _logger = logger;
    }

    [McpServerTool, Description("Displays past blood glucose values (milligrams per deciliter or \"mg/dL\")")]
    public async Task<string> GetBloodGlucose(int count = 12)
    {
        try
        {
            var entries = await _nightscoutService.GetEntriesAsync(count);
            
            if (!entries.Any())
            {
                return "No entries found.";
            }

            var groupedByDate = entries.OrderBy(entry => entry.DateString)
                .GroupBy(entry => NightscoutService.FormatDateTimeString(entry.DateString).Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Entries = g.OrderBy(entry => entry.DateString).ToList()
                });

            var result = string.Join("\n\n", groupedByDate.Select(group =>
                $"Date: {group.Date:yyyy-MM-dd}\n" +
                "Time | Glucose value (mg/dL)\n" +
                string.Join("\n", group.Entries.Select(entry =>
                    $"{NightscoutService.FormatDateTimeString(entry.DateString):HH:mm} | {entry.Sgv}"))));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching latest entries");
            return $"Error retrieving entries: {ex.Message}";
        }
    }
}