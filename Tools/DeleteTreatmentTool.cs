using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Services;

namespace Tools;

[McpServerToolType]
public sealed class DeleteTreatmentTool
{
    private readonly NightscoutService _nightscoutService;
    private readonly ILogger<DeleteTreatmentTool> _logger;

    public DeleteTreatmentTool(NightscoutService nightscoutService, ILogger<DeleteTreatmentTool> logger)
    {
        _nightscoutService = nightscoutService;
        _logger = logger;
    }

    [McpServerTool, Description("Deletes a treatment by its ID")]
    public async Task<string> DeleteTreatment(
        [Description("Treatment ID to delete")] string _id)
    {
        try
        {
            var result = await _nightscoutService.DeleteTreatmentAsync(_id);

            if (result["success"] as bool? == true)
            {
                return $"Treatment deleted successfully: {result["message"]}";
            }
            else
            {
                return $"Failed to delete treatment: {result.GetValueOrDefault("error", "Unknown error")}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting treatment");
            return $"Error deleting treatment: {ex.Message}";
        }
    }
}