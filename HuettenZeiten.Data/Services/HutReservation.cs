using System.Text.Json;
using HuettenZeiten.Data.Models;

namespace HuettenZeiten.Data.Services;

public class HutReservation : IHutService
{
    public async Task<IReadOnlyList<HutUsage>> GetUsages(Hut hut)
    {
        using var httpClient = new HttpClient();
        var json = await httpClient.GetStringAsync($"https://www.hut-reservation.org/api/v1/reservation/getHutAvailability?hutId={hut.Id}&step=WIZARD");

        var usages = JsonSerializer.Deserialize(json, HutReservationUsageJsonContext.Default.HutReservationUsageArray)!
            .ToArray();

        var hutStates = usages.Select(usages => usages.HutStatus).Distinct();
        Console.WriteLine($"HutStatuses: {string.Join(", ", hutStates)}");

        return usages
            .Select(u => u.ToHutUsage())
            .ToArray();
    }
}