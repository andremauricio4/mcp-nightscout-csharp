using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Services;
using Models;
using System.Globalization;

namespace Tools;

[McpServerToolType]
public sealed class GetSensorStartTool
{
    private readonly NightscoutService _nightscoutService;
    private readonly ILogger<GetSensorStartTool> _logger;

    public GetSensorStartTool(NightscoutService nightscoutService, ILogger<GetSensorStartTool> logger)
    {
        _nightscoutService = nightscoutService;
        _logger = logger;
    }

    [McpServerTool, Description("Displays when a new sensor was put on the patient's body and started recording data")]
    public async Task<string> GetSensorStart(int count = 12)
    {
        try
        {
            var treatments = await _nightscoutService.GetTreatmentsAsync("Sensor Start", count);

            if (!treatments.Any())
            {
                return "No sensor start events found.";
            }

            var groupedByDate = treatments
                .OrderBy(t => NightscoutService.FormatDateTimeString(t.CreatedAt))
                .GroupBy(t => NightscoutService.FormatDateTimeString(t.CreatedAt).Date)
                .Select(g =>
                {
                    var dayTreatments = g.OrderBy(t => NightscoutService.FormatDateTimeString(t.CreatedAt)).ToList();
                    var filtered = new List<Treatment>();

                    for (int i = 0; i < dayTreatments.Count; i++)
                    {
                        var currentTime = NightscoutService.FormatDateTimeString(dayTreatments[i].CreatedAt);

                        // If it's the last item, always add it
                        if (i == dayTreatments.Count - 1)
                        {
                            filtered.Add(dayTreatments[i]);
                        }
                        else
                        {
                            var nextTime = NightscoutService.FormatDateTimeString(dayTreatments[i + 1].CreatedAt);
                            var diff = nextTime - currentTime;

                            if (diff.TotalMinutes <= 3)
                            {
                                // Skip current, keep next
                                continue;
                            }
                            else
                            {
                                // No close next, keep current
                                filtered.Add(dayTreatments[i]);
                            }
                        }
                    }

                    return new
                    {
                        Date = g.Key,
                        Treatments = filtered.OrderBy(t => NightscoutService.FormatDateTimeString(t.CreatedAt)).ToList()
                    };
                });

            var result = string.Join("\n\n", groupedByDate.Select(group =>
                $"Date: {group.Date:yyyy-MM-dd}\n" +
                "Time\n" +
                string.Join("\n", group.Treatments.Select(treatment =>
                    $"{NightscoutService.FormatDateTimeString(treatment.CreatedAt):HH:mm}"))));

            return result;




        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sensor start events");
            return $"Error retrieving sensor start events: {ex.Message}";
        }
    }
}
