using System.Text.Json.Serialization;

namespace Scheduler.Models;

public class OutputJson
{
    [JsonPropertyName("sem")] public string Sem { get; set; } = "winter";
    [JsonPropertyName("studies")] public List<string> Studies { get; set; } = new();
    [JsonPropertyName("grades")] public List<object> Grades { get; set; } = new();
    [JsonPropertyName("subjects")] public List<object> Subjects { get; set; } = new();
    [JsonPropertyName("custom")] public List<CustomEntry> Custom { get; set; } = new();
    [JsonPropertyName("selected")] public List<string> Selected { get; set; } = new();
    [JsonPropertyName("deleted")] public List<object> Deleted { get; set; } = new();
    [JsonPropertyName("year")] public int Year { get; set; }
}