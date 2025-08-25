using System.Text.Json.Serialization;

namespace Scheduler.Models;

public class CustomEntry
{
    [JsonPropertyName("id")] public required string Id { get; set; }
    [JsonPropertyName("name")] public required string Name { get; set; }
    [JsonPropertyName("link")] public string Link { get; set; } = string.Empty;
    [JsonPropertyName("day")] public int Day { get; set; }
    [JsonPropertyName("week")] public string Week { get; set; } = "";
    [JsonPropertyName("from")] public double From { get; set; }
    [JsonPropertyName("to")] public double To { get; set; }
    [JsonPropertyName("type")] public string Type { get; set; } = "custom";
    [JsonPropertyName("custom_color")] public string CustomColor { get; set; } = "green";
    [JsonPropertyName("rooms")] public List<string> Rooms { get; set; } = [];
    [JsonPropertyName("info")] public string Info { get; set; } = "";
    [JsonPropertyName("layer")] public int Layer { get; set; } = 1;
    [JsonPropertyName("selected")] public bool Selected { get; set; } = true;
    [JsonPropertyName("deleted")] public bool Deleted { get; set; }
}