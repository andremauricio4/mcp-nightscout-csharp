using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Services;

namespace Tools;

[McpServerToolType]
public sealed class RecordExerciseTool
{
    private readonly NightscoutService _nightscoutService;
    private readonly ILogger<RecordExerciseTool> _logger;

    public RecordExerciseTool(NightscoutService nightscoutService, ILogger<RecordExerciseTool> logger)
    {
        _nightscoutService = nightscoutService;
        _logger = logger;
    }

    [McpServerTool, Description("Records exercise activity")]
    public async Task<string> RecordExercise(
        [Description("Duration of the exercise in minutes")] int? duration,
        [Description("Name or description of the exercise")] string? notesDescription = null,
        [Description("Event time in Lisbon/Europe local time formatted as 'yyyy-MM-dd HH:mm' (optional, defaults to current time)")] string? eventTime = null)
    {
        try
        {
            var result = await _nightscoutService.AddTreatmentCoreAsync(
                eventType: "Exercise",
                duration: duration,
                notesDescription: notesDescription,
                eventTime: eventTime
            );

            if (result["success"] as bool? == true)
            {
                return $"Exercise recorded successfully. Treatment ID: {result.GetValueOrDefault("treatment_id", "N/A")}";
            }
            else
            {
                return $"Failed to record exercise: {result.GetValueOrDefault("error", "Unknown error")}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording exercise");
            return $"Error recording exercise: {ex.Message}";
        }
    }
}