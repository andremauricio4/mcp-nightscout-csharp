using System.Text.Json.Serialization;

namespace Models
{
    public class Treatment
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("eventType")]
        public string EventType { get; set; } = string.Empty;

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("carbs")]
        public double? Carbs { get; set; }

        [JsonPropertyName("insulin")]
        public double? Insulin { get; set; }

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        [JsonPropertyName("glucose")]
        public int? Glucose { get; set; }

        [JsonPropertyName("enteredBy")]
        public string EnteredBy { get; set; } = string.Empty;

        [JsonPropertyName("glucoseType")]
        public string GlucoseType { get; set; } = string.Empty;

        [JsonPropertyName("duration")]
        public double? Duration { get; set; }

        [JsonPropertyName("absolute")]
        public double? Absolute { get; set; }

        [JsonPropertyName("units")]
        public string? Units { get; set; }
    }
}