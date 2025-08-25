using System.Globalization;
using System.Text.Json;
using Cocona;
using CsvHelper;
using CsvHelper.Configuration;
using Scheduler.Models;

namespace Scheduler;

internal class Generator
{
    private static readonly string[] Colors = ["green", "blue", "yellow", "darkblue", "purple", "red", "orange", "brown", "gray"];

    public void Run(
        [Option('i', Description = "Input CSV file path")]
        string csvPath,
        [Option('s', Description = "Semester (e.g., winter)")]
        string semester,
        [Option('y', Description = "Year (e.g., 2025)")]
        int year,
        [Option("free-days", Description = "Target number of free days per week (optional)")]
        int freeDaysTarget = 0
    )
    {
        var csvEntries = LoadCsvEntries(csvPath);
        var subjectsByCode = GroupSubjectsByCode(csvEntries);
        var colorMap = AssignColors(subjectsByCode.Keys.ToList());

        var allSchedules = GenerateSchedules(subjectsByCode.Values.ToList(), 0, new List<CsvEntry>());
        allSchedules = ApplyFreeDaysTarget(allSchedules, freeDaysTarget);

        WriteSchedulesToJson(allSchedules, semester, year, colorMap);

        Console.WriteLine(allSchedules.Count == 0
            ? "There are 0 possible combinations"
            : $"{allSchedules.Count} Schedule{(allSchedules.Count > 1 ? "s" : "")} generated");
    }

    private static List<CsvEntry> LoadCsvEntries(string csvPath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<CsvEntryMap>();

        return csv.GetRecords<CsvEntry>().ToList();
    }

    private static Dictionary<string, List<CsvEntry>> GroupSubjectsByCode(List<CsvEntry> csvEntries)
    {
        return csvEntries.GroupBy(e => e.Code)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    private static Dictionary<string, string> AssignColors(List<string> subjectCodes)
    {
        var colorMap = new Dictionary<string, string>();
        for (var i = 0; i < subjectCodes.Count; i++)
        {
            colorMap[subjectCodes[i]] = Colors[i % Colors.Length];
        }

        return colorMap;
    }

    private static List<List<CsvEntry>> ApplyFreeDaysTarget(List<List<CsvEntry>> schedules, int freeDaysTarget)
    {
        if (freeDaysTarget <= 0) return schedules;

        return schedules.Where(schedule =>
            5 - schedule.Select(c => c.Day).Distinct().Count() >= freeDaysTarget
        ).ToList();
    }

    private static List<List<CsvEntry>> GenerateSchedules(List<List<CsvEntry>> subjectGroups, int index, List<CsvEntry> current)
    {
        var result = new List<List<CsvEntry>>();

        if (index >= subjectGroups.Count)
        {
            result.Add([..current]);
            return result;
        }

        foreach (var entry in subjectGroups[index].Where(entry => !HasCollision(current, entry)))
        {
            current.Add(entry);
            result.AddRange(GenerateSchedules(subjectGroups, index + 1, current));
            current.RemoveAt(current.Count - 1);
        }

        return result;
    }

    private static bool HasCollision(List<CsvEntry> current, CsvEntry candidate)
    {
        return current.Any(c => c.Day == candidate.Day && c.To > candidate.From && candidate.To > c.From);
    }
    
    private static void WriteSchedulesToJson(
        List<List<CsvEntry>> allSchedules,
        string semester,
        int year,
        Dictionary<string, string> colorMap
    )
    {
        if (Directory.Exists("output")) Directory.Delete("output", true);
        Directory.CreateDirectory("output");

        var random = new Random();
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IncludeFields = true
        };

        var fileIndex = 1;
        foreach (var outputJson in allSchedules.Select(schedule => schedule.Select(e => new CustomEntry
                 {
                     Id = "CUST_" + random.Next(1000000000, int.MaxValue),
                     Name = e.Title,
                     Link = $"{e.Code}-{e.Title}",
                     Day = (int)e.Day,
                     Week = e.Weeks,
                     From = e.From,
                     To = e.To,
                     Rooms = [e.Room],
                     CustomColor = colorMap[e.Code]
                 }).ToList()).Select(customEntries => new OutputJson
                 {
                     Sem = semester,
                     Year = year,
                     Custom = customEntries,
                     Selected = customEntries.Where(c => c.Selected).Select(c => c.Id).ToList(),
                     Studies = []
                 }))
        {
            File.WriteAllText($"output/schedule_{fileIndex}.json", JsonSerializer.Serialize(outputJson, jsonOptions));
            fileIndex++;
        }
    }
}