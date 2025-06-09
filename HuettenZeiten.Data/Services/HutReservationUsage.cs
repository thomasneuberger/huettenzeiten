using HuettenZeiten.Data.Models;

namespace HuettenZeiten.Data.Services;

internal class HutReservationUsage
{
    public IDictionary<int, int> FreeBedsPerCategory { get; init; }
    public int? FreeBeds { get; init; }
    public Status HutStatus { get; init; }
    public string Date { get; init; }
    public string DateFormatted { get; init; }
    public int TotalSleepingPlaces { get; init; }
    public string Percentage { get; init; }

    internal enum Status
    {
        SERVICED,
        CLOSED
    }

    internal HutUsage ToHutUsage()
    {
        return new HutUsage
        {
            IsOpen = HutStatus == Status.SERVICED,
            Date = DateOnly.FromDateTime(DateTime.Parse(Date)),
            FreeBeds = FreeBeds ?? 0
        };
    }
}
