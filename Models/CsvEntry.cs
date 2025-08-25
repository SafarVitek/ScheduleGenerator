namespace Scheduler.Models;

class CsvEntry
{
    public required string Code { get; set; }
    public required string Title { get; set; }
    public Day Day { get; set; }
    public double From { get; set; }
    public double To { get; set; }
    public string Weeks { get; set; } = string.Empty;
    public required string Room { get; set; }
}