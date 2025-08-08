using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Services;

namespace Tools;

[McpServerToolType]
public sealed class RecordBasalInsulinTool
{
    private readonly NightscoutService _nightscoutService;
    private readonly ILogger<RecordBasalInsulinTool> _logger;

    public RecordBasalInsulinTool(NightscoutService nightscoutService, ILogger<RecordBasalInsulinTool> logger)
    {
        _nightscoutService = nightscoutService;
        _logger = logger;
    }

    [McpServerTool, Description("Records slow-acting/basal insulin administration")]
    public async Task<string> RecordBasalInsulin(
        [Description("Amount of slow-acting/basal insulin in international units")] double? absolute,
        [Description("Duration of the insulin in minutes (default: 1440 minutes = 24 hours)")] int? duration = 1440,
        [Description("Name or description of the insulin")] string? notesDescription = null,
        [Description("Event time in Lisbon/Europe local time formatted as 'yyyy-MM-dd HH:mm' (optional, defaults to current time)")] string? eventTime = null)
    {
        try
        {
            var result = await _nightscoutService.AddTreatmentCoreAsync(
                eventType: "Temp Basal",
                absolute: absolute,
                duration: duration,
                notesDescription: notesDescription,
                eventTime: eventTime
            );

            if (result["success"] as bool? == true)
            {
                return $"Basal insulin recorded successfully. Treatment ID: {result.GetValueOrDefault("treatment_id", "N/A")}";
            }
            else
            {
                return $"Failed to record basal insulin: {result.GetValueOrDefault("error", "Unknown error")}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording basal insulin");
            return $"Error recording basal insulin: {ex.Message}";
        }
    }
}