using System.Globalization;
using System.Text.Json;
using Cocona;
using Scheduler.Models;

namespace Scheduler;

internal static class Program
{
    private static void Main(string[] args)
    {
        CoconaApp.Run<App>(args);
    }
}

internal class App
{
    private static readonly string[] Colors = ["green", "blue", "yellow", "darkblue", "purple", "red", "orange", "brown", "gray"];

    public void Run(
        [Option('i', Description = "Input CSV file path")] string csv,
        [Option('s', Description = "Semester (e.g., winter)")] string semester,
        [Option('y', Description = "Year (e.g., 2025)")] int year,
        [Option("free-days", Description = "Target number of free days per week (optional)")] int freeDaysTarget = 0
    )
    {
        var csvEntries = File.ReadAllLines(csv)
            .Skip(1)
            .Select(line => line.Split(','))
            .Select(parts => new CsvEntry
            {
                Code = parts[0],
                Title = parts[1],
                Day = Enum.Parse<Day>(parts[2], ignoreCase: true),
                From = TimeToSlot(parts[3]),
                To = TimeToSlot(parts[4]),
                Weeks = parts[5],
                Room = parts[6]
            })
            .ToList();

        var subjectsByCode = csvEntries.GroupBy(e => e.Code)
            .ToDictionary(g => g.Key, g => g.ToList());

        var subjectCodes = subjectsByCode.Select(l => l.Key).Distinct().ToList();
        var colorMap = new Dictionary<string, string>();
        for (var i = 0; i < subjectCodes.Count; i++)
        {
            colorMap[subjectCodes[i]] = Colors[i % Colors.Length];
        }

        var allSchedules = GenerateSchedules(subjectsByCode.Values.ToList(), 0, []);

        if (freeDaysTarget > 0)
        {
            allSchedules = allSchedules
                .Where(schedule => 5 - schedule.Select(c => c.Day).Distinct().Count() >= freeDaysTarget)
                .ToList();
        }

        var fileIndex = 1;
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IncludeFields = true
        };

        if (Directory.Exists("output"))
        {
            Directory.Delete("output", recursive: true);
        }
        Directory.CreateDirectory("output");

        var random = new Random();

        foreach (var json in from schedule in allSchedules
                 select schedule.Select(e => new CustomEntry
                 {
                     Id = "CUST_" + random.Next(1000000000, int.MaxValue),
                     Name = e.Title,
                     Link = $"{e.Code}-{e.Title}",
                     Day = (int)e.Day,
                     Week = e.Weeks ?? "",
                     From = e.From,
                     To = e.To,
                     Rooms = [e.Room],
                     CustomColor = colorMap[e.Code],
                     Type = "custom",
                     Info = "",
                     Layer = 1,
                     Selected = true,
                     Deleted = false
                 }).ToList()
                 into customEntries
                 let selectedIds = customEntries.Where(c => c.Selected).Select(c => c.Id).ToList()
                 select new OutputJson
                 {
                     Sem = semester,
                     Year = year,
                     Custom = customEntries,
                     Selected = selectedIds,
                     Studies = []
                 }
                 into output
                 select JsonSerializer.Serialize(output, jsonOptions))
        {
            File.WriteAllText($"output/schedule_{fileIndex}.json", json);
            fileIndex++;
        }

        Console.WriteLine(allSchedules.Count == 0
            ? "There are 0 possible combinations"
            : $"{allSchedules.Count} Schedule{(allSchedules.Count > 1 ? "s" : "")} generated");
    }

    private static double TimeToSlot(string time)
    {
        var ts = TimeSpan.ParseExact(time, "hh\\:mm", CultureInfo.InvariantCulture);
        const double baseHour = 7;
        return (ts.TotalMinutes - baseHour * 60) / 60.0;
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
}