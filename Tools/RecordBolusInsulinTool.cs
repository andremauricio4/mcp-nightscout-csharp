using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Services;

namespace Tools;

[McpServerToolType]
public sealed class RecordBolusInsulinTool
{
    private readonly NightscoutService _nightscoutService;
    private readonly ILogger<RecordBolusInsulinTool> _logger;

    public RecordBolusInsulinTool(NightscoutService nightscoutService, ILogger<RecordBolusInsulinTool> logger)
    {
        _nightscoutService = nightscoutService;
        _logger = logger;
    }

    [McpServerTool, Description("Records fast-acting/bolus insulin administration")]
    public async Task<string> RecordBolusInsulin(
        [Description("Amount of fast-acting/bolus insulin in international units")] double? insulin,
        [Description("Name or description of the insulin")] string? notesDescription = null,
        [Description("Event time in Lisbon/Europe local time formatted as 'yyyy-MM-dd HH:mm' (optional, defaults to current time)")] string? eventTime = null)
    {
        try
        {
            var result = await _nightscoutService.AddTreatmentCoreAsync(
                eventType: "Bolus",
                insulin: insulin,
                notesDescription: notesDescription,
                eventTime: eventTime
            );

            if (result["success"] as bool? == true)
            {
                return $"Bolus insulin recorded successfully. Treatment ID: {result.GetValueOrDefault("treatment_id", "N/A")}";
            }
            else
            {
                return $"Failed to record bolus insulin: {result.GetValueOrDefault("error", "Unknown error")}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording bolus insulin");
            return $"Error recording bolus insulin: {ex.Message}";
        }
    }
}