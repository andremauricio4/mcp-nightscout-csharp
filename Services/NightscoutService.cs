using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Globalization;
using Models;

namespace Services;

public class NightscoutService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NightscoutService> _logger;

    public const string NIGHTSCOUT_URL = "http://192.168.1.117:1337";
    public const string NIGHTSCOUT_TOKEN = "651f37427b5a4a006129328f";

    public static readonly TimeZoneInfo LisbonTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Lisbon");

    public NightscoutService(HttpClient httpClient, ILogger<NightscoutService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(NIGHTSCOUT_URL);
        var apiHash = ComputeSha1Hash(NIGHTSCOUT_TOKEN);
        _httpClient.DefaultRequestHeaders.Add("api-secret", apiHash);
    }

    private static string ComputeSha1Hash(string input)
    {
        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static string FormatDateTime(DateTime dateTime)
    {
        var lisbonTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime.ToUniversalTime(), LisbonTimeZone);
        return lisbonTime.ToString("yyyy-MM-dd HH:mm");
    }

    public static DateTime FormatDateTimeString(string utcDateString)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.ParseExact(
                utcDateString,
                "yyyy-MM-ddTHH:mm:ss.fffZ",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal
            ),
            LisbonTimeZone
        );
    }

    private static (string StartUtcStr, string EndUtcStr) GetUtcDateRange(string? startTime, string? endTime)
    {
        DateTime utcStart, utcEnd;

        if (!string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
        {
            var localStart = DateTime.ParseExact(startTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            var localEnd = DateTime.ParseExact(endTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

            utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart, LisbonTimeZone);
            utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd, LisbonTimeZone);
        }
        else
        {
            utcEnd = DateTime.UtcNow;
            utcStart = utcEnd.AddDays(-4);
        }

        return (
            utcStart.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture),
            utcEnd.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture)
        );
    }

    private static (string StartUtcStr, string EndUtcStr) GetUtcDateRangeFromDays(int howManyDaysStart, int howManyDaysEnd)
    {
        var utcNow = DateTime.UtcNow;
        var utcStart = utcNow.AddDays(howManyDaysStart);
        var utcEnd = utcNow.AddDays(howManyDaysEnd);

        return (
            utcStart.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture),
            utcEnd.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture)
        );
    }

    public async Task<List<Treatment>> GetTreatmentsAsync(string? eventType = null, int count = 50, string? startTime = null, string? endTime = null)
    {
        var (startTimeStr, endTimeStr) = GetUtcDateRange(startTime, endTime);
        return await GetTreatmentsCoreAsync(eventType, count, startTimeStr, endTimeStr);
    }

    public async Task<List<Treatment>> GetTreatmentsByDaysAsync(string? eventType = null, int count = 50, int howManyDaysStart = -4, int howManyDaysEnd = 0)
    {
        var (startTimeStr, endTimeStr) = GetUtcDateRangeFromDays(howManyDaysStart, howManyDaysEnd);
        return await GetTreatmentsCoreAsync(eventType, count, startTimeStr, endTimeStr);
    }

    private async Task<List<Treatment>> GetTreatmentsCoreAsync(string? eventType, int count, string startTimeStr, string endTimeStr)
    {
        var url = $"/api/v1/treatments.json?count={count}&find[created_at][$gte]={startTimeStr}&find[created_at][$lte]={endTimeStr}";
        
        if (!string.IsNullOrEmpty(eventType))
        {
            url += $"&find[eventType]={eventType}";
        }
        
        var response = await _httpClient.GetAsync(url);
        
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Unauthorized access to Nightscout. Please check your NIGHTSCOUT_TOKEN configuration.");
        }
        
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Treatment[]>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        })?.ToList() ?? new List<Treatment>();
    }

    public async Task<List<Entry>> GetEntriesAsync(int count, string? startTime = null, string? endTime = null)
    {
        var (startTimeStr, endTimeStr) = GetUtcDateRange(startTime, endTime);

        var url = $"/api/v1/entries.json?count={count}&find[dateString][$gte]={startTimeStr}&find[dateString][$lte]={endTimeStr}";
        var response = await _httpClient.GetAsync(url);
        
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Unauthorized access to Nightscout. Please check your NIGHTSCOUT_TOKEN configuration.");
        }
        
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Entry[]>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        })?.ToList() ?? new List<Entry>();
    }

    public async Task<Dictionary<string, object>> AddTreatmentCoreAsync(
        string eventType, 
        double? insulin = null, 
        double? carbs_g = null, 
        double? absolute = null, 
        int? duration = null, 
        string? notesDescription = null, 
        string? enteredBy = null, 
        string? eventTime = null, 
        string? units = null, 
        string? glucoseType = null, 
        int? glucose = null)
    {
        // Handle eventTime - convert from local Lisbon time to UTC, or use current UTC if null
        DateTime utcDateTime;
        if (!string.IsNullOrEmpty(eventTime))
        {
            var localDateTime = DateTime.ParseExact(eventTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, LisbonTimeZone);
        }
        else
        {
            utcDateTime = DateTime.UtcNow;
        }

        // Prepare treatment data
        var treatmentData = new Dictionary<string, object>
        {
            ["eventType"] = eventType,
            ["created_at"] = utcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture)
        };

        // Add optional parameters if they have values
        if (insulin.HasValue) treatmentData["insulin"] = insulin.Value;
        if (carbs_g.HasValue) treatmentData["carbs"] = carbs_g.Value;
        if (absolute.HasValue) treatmentData["absolute"] = absolute.Value;
        if (duration.HasValue) treatmentData["duration"] = duration.Value;
        if (!string.IsNullOrEmpty(notesDescription)) treatmentData["notes"] = notesDescription;
        if (!string.IsNullOrEmpty(enteredBy)) treatmentData["enteredBy"] = enteredBy;
        if (!string.IsNullOrEmpty(units)) treatmentData["units"] = units;
        if (!string.IsNullOrEmpty(glucoseType)) treatmentData["glucoseType"] = glucoseType;
        if (glucose.HasValue) treatmentData["glucose"] = glucose.Value;

        // Make HTTP POST request
        var jsonContent = JsonSerializer.Serialize(new[] { treatmentData }, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/api/v1/treatments", content);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Unauthorized access to Nightscout. Please check your NIGHTSCOUT_TOKEN configuration.");
        }

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Dictionary<string, object>[]>(responseJson);

        return new Dictionary<string, object>
        {
            ["success"] = true,
            ["message"] = "Treatment added successfully",
            ["treatment_id"] = result?[0]?.ContainsKey("_id") == true ? result[0]["_id"] : null,
            ["data"] = treatmentData
        };
    }

    public async Task<Dictionary<string, object>> DeleteTreatmentAsync(string _id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/v1/treatments/{_id}");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Unauthorized access to Nightscout. Please check your NIGHTSCOUT_TOKEN configuration.");
            }

            response.EnsureSuccessStatusCode();

            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["message"] = $"Treatment {_id} deleted successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting treatment {TreatmentId}", _id);
            return new Dictionary<string, object>
            {
                ["success"] = false,
                ["error"] = $"Failed to delete treatment: {ex.Message}"
            };
        }
    }
}
