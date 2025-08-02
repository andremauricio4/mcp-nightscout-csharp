using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Services;
using Models;

namespace Tools;

[McpServerToolType]
public sealed class GetLatestNotesTool
{
    private readonly NightscoutService _nightscoutService;
    private readonly ILogger<GetLatestNotesTool> _logger;

    public GetLatestNotesTool(NightscoutService nightscoutService, ILogger<GetLatestNotesTool> logger)
    {
        _nightscoutService = nightscoutService;
        _logger = logger;
    }

    [McpServerTool, Description("Displays the logged notes")]
    public async Task<string> GetNotes(int count = 12)
    {
        try
        {
            var treatments = await _nightscoutService.GetTreatmentsAsync("Note", count);
            
            if (!treatments.Any())
            {
                return "No notes found.";
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
                "Time | Note\n" +
                string.Join("\n", group.Treatments.Select(treatment =>
                    $"{NightscoutService.FormatDateTimeString(treatment.CreatedAt):HH:mm} | {treatment.Notes}"))));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching latest notes");
            return $"Error retrieving notes: {ex.Message}";
        }
    }
}