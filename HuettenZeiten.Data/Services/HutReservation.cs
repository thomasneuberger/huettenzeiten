using System.Net;
using System.Text.Json;
using HuettenZeiten.Data.Models;
using Microsoft.Extensions.Logging;

namespace HuettenZeiten.Data.Services;

public class HutReservation : IHutService
{
    private readonly ILogger<HutReservation> _logger;

    public HutReservation(ILogger<HutReservation> logger)
    {
        _logger = logger;
    }

    public async Task<string?> GetHutName(int hutId)
    {
        try
        {
            _logger.LogDebug("Fetching hut name for hutId: {HutId}", hutId);
            using var httpClient = new HttpClient();
            var json = await httpClient.GetStringAsync($"https://www.hut-reservation.org/api/v1/reservation/hutInfo/{hutId}");

            var hutInfo = JsonSerializer.Deserialize(json, HutReservationJsonContext.Default.HutInformation)!;

            _logger.LogInformation("Successfully retrieved hut name '{HutName}' for hutId: {HutId}", hutInfo.HutName, hutId);
            return hutInfo.HutName;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            _logger.LogError(ex, "Failed to retrieve hut name. Hut with ID {HutId} not found (HTTP 400)", hutId);
            Console.Error.WriteLine($"Fehler beim Abrufen des Hüttennamens. Möglicherweise wurde die Hütte mit der ID {hutId} nicht gefunden.");
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization failed when retrieving hut information for hutId: {HutId}", hutId);
            Console.Error.WriteLine($"Fehler beim Abrufen der Hütteninformationen: {ex.Message}");
            return null;
        }
    }

    public async Task<IReadOnlyList<HutUsage>> GetUsages(Hut hut)
    {
        try
        {
            _logger.LogDebug("Fetching usage data for hut '{HutName}' (ID: {HutId})", hut.Name, hut.Id);
            using var httpClient = new HttpClient();
            var json = await httpClient.GetStringAsync($"https://www.hut-reservation.org/api/v1/reservation/getHutAvailability?hutId={hut.Id}&step=WIZARD");

            var usages = JsonSerializer.Deserialize(json, HutReservationJsonContext.Default.HutReservationUsageArray)!
                .ToArray();

            var hutUsages = usages
                .Select(u => u.ToHutUsage())
                .ToArray();

            _logger.LogInformation("Successfully retrieved {UsageCount} usage entries for hut '{HutName}' (ID: {HutId})", hutUsages.Length, hut.Name, hut.Id);
            return hutUsages;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed when retrieving usage data for hut '{HutName}' (ID: {HutId}). Status Code: {StatusCode}", hut.Name, hut.Id, ex.StatusCode);
            Console.Error.WriteLine($"Fehler beim Abrufen der Nutzungsdaten für die Hütte mit der ID {hut.Id}.");
            return Array.Empty<HutUsage>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization failed when processing usage data for hut '{HutName}' (ID: {HutId})", hut.Name, hut.Id);
            Console.Error.WriteLine($"Fehler beim Deserialisieren der Nutzungsdaten: {ex.Message}");
            return Array.Empty<HutUsage>();
        }
    }
}