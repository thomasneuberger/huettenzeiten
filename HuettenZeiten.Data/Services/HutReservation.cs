using System.Net;
using System.Text.Json;
using HuettenZeiten.Data.Models;

namespace HuettenZeiten.Data.Services;

public class HutReservation : IHutService
{
    public async Task<string?> GetHutName(int hutId)
    {
        try
        {
            using var httpClient = new HttpClient();
            var json = await httpClient.GetStringAsync($"https://www.hut-reservation.org/api/v1/reservation/hutInfo/{hutId}");

            var hutInfo = JsonSerializer.Deserialize(json, HutReservationJsonContext.Default.HutInformation)!;

            return hutInfo.HutName;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            Console.Error.WriteLine($"Fehler beim Abrufen des Hüttennamens. Möglicherweise wurde die Hütte mit der ID {hutId} nicht gefunden.");
            return null;
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"Fehler beim Abrufen der Hütteninformationen: {ex.Message}");
            return null;
        }
    }

    public async Task<IReadOnlyList<HutUsage>> GetUsages(Hut hut)
    {
        try
        {
            using var httpClient = new HttpClient();
            var json = await httpClient.GetStringAsync($"https://www.hut-reservation.org/api/v1/reservation/getHutAvailability?hutId={hut.Id}&step=WIZARD");

            var usages = JsonSerializer.Deserialize(json, HutReservationJsonContext.Default.HutReservationUsageArray)!
                .ToArray();

            return usages
                .Select(u => u.ToHutUsage())
                .ToArray();
        }
        catch (HttpRequestException)
        {
            Console.Error.WriteLine($"Fehler beim Abrufen der Nutzungsdaten für die Hütte mit der ID {hut.Id}.");
            return Array.Empty<HutUsage>();
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"Fehler beim Deserialisieren der Nutzungsdaten: {ex.Message}");
            return Array.Empty<HutUsage>();
        }
    }
}