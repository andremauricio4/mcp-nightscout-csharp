using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Services;

namespace Tools;

[McpServerToolType]
public sealed class RecordFingerPrickCapillaryGlucometerCheckTool
{
    private readonly NightscoutService _nightscoutService;
    private readonly ILogger<RecordFingerPrickCapillaryGlucometerCheckTool> _logger;

    public RecordFingerPrickCapillaryGlucometerCheckTool(NightscoutService nightscoutService, ILogger<RecordFingerPrickCapillaryGlucometerCheckTool> logger)
    {
        _nightscoutService = nightscoutService;
        _logger = logger;
    }

    [McpServerTool, Description("Records finger prick capillary glucometer blood glucose check")]
    public async Task<string> RecordFingerPrickCapillaryGlucometerCheck(
        [Description("Glucometer blood glucose test result in mg/dl")] int glucose,
        [Description("Event time in Lisbon/Europe local time formatted as 'yyyy-MM-dd HH:mm' (optional, defaults to current time)")] string? eventTime = null)
    {
        try
        {
            var result = await _nightscoutService.AddTreatmentCoreAsync(
                eventType: "BG Check",
                glucose: glucose,
                glucoseType: "Finger",
                units: "mg/dl",
                eventTime: eventTime
            );

            if (result["success"] as bool? == true)
            {
                return $"Blood glucose check recorded successfully. Treatment ID: {result.GetValueOrDefault("treatment_id", "N/A")}";
            }
            else
            {
                return $"Failed to record blood glucose check: {result.GetValueOrDefault("error", "Unknown error")}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording blood glucose check");
            return $"Error recording blood glucose check: {ex.Message}";
        }
    }
}