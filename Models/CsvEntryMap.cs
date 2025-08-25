using System.Globalization;
using CsvHelper.Configuration;

namespace Scheduler.Models;

public sealed class CsvEntryMap : ClassMap<CsvEntry>
{
    public CsvEntryMap()
    {
        Map(m => m.Code).Name("Code");
        Map(m => m.Title).Name("Title");
        Map(m => m.Day).Name("Day").Convert(row =>
        {
            var dayString = row.Row.GetField("Day");
            return Enum.Parse<Day>(dayString ?? throw new ArgumentNullException(), ignoreCase: true);
        });
        Map(m => m.From).Name("From").Convert(row => TimeToSlot(row.Row.GetField("From")));
        Map(m => m.To).Name("To").Convert(row => TimeToSlot(row.Row.GetField("To")));
        Map(m => m.Weeks).Name("Weeks").Default(string.Empty);
        Map(m => m.Room).Name("Room");
    }

    private static double TimeToSlot(string? time)
    {
        ArgumentNullException.ThrowIfNull(time);
        var ts = TimeSpan.ParseExact(time, "hh\\:mm", CultureInfo.InvariantCulture);
        const double baseHour = 7;
        return (ts.TotalMinutes - baseHour * 60) / 60.0;
    }
}