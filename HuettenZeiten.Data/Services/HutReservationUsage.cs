using HuettenZeiten.Data.Models;

namespace HuettenZeiten.Data.Services;

internal class HutReservationUsage
{
    public IDictionary<int, int> FreeBedsPerCategory { get; init; } = new Dictionary<int, int>();

    public int? FreeBeds { get; init; }

    public string HutStatus { get; init; } = "UNKNOWN";

    public required string Date { get; init; }

    public required string DateFormatted { get; init; }

    public int TotalSleepingPlaces { get; init; }

    public required string Percentage { get; init; }

    internal HutUsage ToHutUsage()
    {
        return new HutUsage
        {
            IsOpen = HutStatus == "SERVICED",
            Date = DateOnly.FromDateTime(DateTime.Parse(Date)),
            FreeBeds = FreeBeds ?? 0
        };
    }
}
