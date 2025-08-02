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
        public int? Carbs { get; set; }

        [JsonPropertyName("insulin")]
        public double? Insulin { get; set; }

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        [JsonPropertyName("glucose")]
        public int? Glucose { get; set; }

        [JsonPropertyName("enteredBy")]
        public string EnteredBy { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("duration")]
        public double? Duration { get; set; }

        [JsonPropertyName("rate")]
        public double? Rate { get; set; }

        [JsonPropertyName("basal")]
        public double? Basal { get; set; }

        [JsonPropertyName("absolute")]
        public double? Absolute { get; set; }

        [JsonPropertyName("units")]
        public string? Units { get; set; }
    }
}