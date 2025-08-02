using System.Text.Json.Serialization;

namespace Models;

public class Entry
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("sgv")]
    public int Sgv { get; set; }
    
    [JsonPropertyName("date")]
    public long Date { get; set; }
    
    [JsonPropertyName("dateString")]
    public string DateString { get; set; } = string.Empty;
    
    [JsonPropertyName("direction")]
    public string Direction { get; set; } = string.Empty;
    
    [JsonPropertyName("filtered")]
    public int Filtered { get; set; }
}