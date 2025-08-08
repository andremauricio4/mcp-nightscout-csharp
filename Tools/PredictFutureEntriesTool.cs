using ModelContextProtocol.Server;
using System.ComponentModel;
using Services;
using Models;

namespace Tools;

[McpServerToolType]
public sealed class PredictFutureEntriesTool
{
    private readonly NightscoutService _nightscoutService;
    private readonly ILogger<PredictFutureEntriesTool> _logger;

    // AR2 forecast constants
    private const double BG_REF = 140.0;
    private const int BG_MIN = 36;
    private const int BG_MAX = 400;
    private static readonly double[] AR = { -0.723, 1.716 };
    private static readonly double[] ConeSteps = { 0.020, 0.041, 0.061, 0.081, 0.099, 0.116, 0.132, 0.146, 0.159, 0.171, 0.182, 0.192 };

    public PredictFutureEntriesTool(NightscoutService nightscoutService, ILogger<PredictFutureEntriesTool> logger)
    {
        _nightscoutService = nightscoutService;
        _logger = logger;
    }

    [McpServerTool, Description("Gets the predicted blood glucose values (milligrams per deciliter or \"mg/dL\")")]
    public async Task<string> PredictFutureEntries(
        [Description("Number of past entries to be used for the prediction (default: 12)")] int pastEntries = 12,
        [Description("Prediction window in minutes (default: 60)")] int forecastMinutes = 60,
        [Description("If the result should include the confidence bands (default: false)")] bool cone = false,
        [Description("Factor for the confidence bands (default: 2.0)")] double coneFactor = 2.0)
    {
        try
        {
            var entries = await _nightscoutService.GetEntriesAsync(pastEntries);
            
            if (!entries.Any())
            {
                return "No entries found to base forecast on.";
            }

            if (entries.Count() < 2)
            {
                return "At least two readings are required for AR2 forecast.";
            }

            // Convert entries to BG readings for forecast
            var readings = entries
                .Select(entry => new BgReading(
                    NightscoutService.FormatDateTimeString(entry.DateString), 
                    entry.Sgv))
                .OrderBy(r => r.Timestamp)
                .ToList();

            var forecastPoints = GenerateAr2Forecast(readings, forecastMinutes, cone, coneFactor);

            if (!forecastPoints.Any())
            {
                return "Unable to generate forecast.";
            }

            var groupedByDate = forecastPoints.OrderBy(point => point.ForecastTime)
                .GroupBy(point => point.ForecastTime.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Points = g.OrderBy(point => point.ForecastTime).ToList()
                });

            if (cone)
            {
                var result = string.Join("\n\n", groupedByDate.Select(group =>
                    $"Date: {group.Date:yyyy-MM-dd}\n" +
                    "Time | glucose (mg/dL) | lower bound | upper bound\n" +
                    string.Join("\n", group.Points.Select(point =>
                        $"{point.ForecastTime:HH:mm} | {point.Value} | {point.LowerBound} | {point.UpperBound}"))));
                
                return result;
            }
            else
            {
                var result = string.Join("\n\n", groupedByDate.Select(group =>
                    $"Date: {group.Date:yyyy-MM-dd}\n" +
                    "Time | glucose (mg/dL)\n" +
                    string.Join("\n", group.Points.Select(point =>
                        $"{point.ForecastTime:HH:mm} | {point.Value}"))));
                
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating forecast");
            return $"Error generating forecast: {ex.Message}";
        }
    }

    private List<ForecastPoint> GenerateAr2Forecast(List<BgReading> readings, int forecastMinutes, bool cone = false, double coneFactor = 2.0)
    {
        if (readings == null || readings.Count < 2)
            return new List<ForecastPoint>();

        var sorted = readings.OrderBy(r => r.Timestamp).ToList();
        var latest = sorted[^1];
        var previous = sorted[^2];

        double curr = Math.Log(latest.Value / BG_REF);
        double prev = Math.Log(previous.Value / BG_REF);

        var forecastPoints = new List<ForecastPoint>();
        var forecastTime = latest.Timestamp;
        int steps = forecastMinutes / 5; // 5-minute intervals

        // Limit steps to available cone steps when using cone bands
        if (cone)
        {
            steps = Math.Min(steps, ConeSteps.Length);
        }

        for (int i = 0; i < steps; i++)
        {
            forecastTime = forecastTime.AddMinutes(5);

            // AR2 calculation in log-space
            double next = AR[0] * prev + AR[1] * curr;

            if (cone)
            {
                // Cone bands in log-space
                double lowerLog = next - coneFactor * ConeSteps[i];
                double upperLog = next + coneFactor * ConeSteps[i];

                int center = ToMgdl(next);
                int lower = ToMgdl(lowerLog);
                int upper = ToMgdl(upperLog);

                forecastPoints.Add(new ForecastPoint(forecastTime, center, lower, upper));
            }
            else
            {
                // Standard forecast without cone bands
                int predictedValue = ToMgdl(next);
                forecastPoints.Add(new ForecastPoint(forecastTime, predictedValue));
            }

            // Shift values for next iteration
            prev = curr;
            curr = next;
        }

        return forecastPoints;
    }

    private static int ToMgdl(double logValue)
    {
        return Math.Clamp((int)Math.Round(BG_REF * Math.Exp(logValue)), BG_MIN, BG_MAX);
    }

    private record BgReading(DateTime Timestamp, int Value);
    private record ForecastPoint(DateTime ForecastTime, int Value, int LowerBound = 0, int UpperBound = 0);
}