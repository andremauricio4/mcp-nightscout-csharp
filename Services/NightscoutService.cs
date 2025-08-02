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
    public const string NIGHTSCOUT_TOKEN = "gggggggggggggggggggggg";

    private static readonly TimeZoneInfo LisbonTimeZone =
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

    public async Task<List<Treatment>> GetTreatmentsAsync(string eventType, int count, string? startTime = null, string? endTime = null)
    {
        var (startTimeStr, endTimeStr) = GetUtcDateRange(startTime, endTime);

        var url = $"/api/v1/treatments.json?find[eventType]={eventType}&count={count}&find[created_at][$gte]={startTimeStr}&find[created_at][$lte]={endTimeStr}";
        var response = await _httpClient.GetAsync(url);
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
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Entry[]>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        })?.ToList() ?? new List<Entry>();
    }
}
