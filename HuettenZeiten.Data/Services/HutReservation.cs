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
        const int maxRetries = 3;
        const int initialDelayMs = 1000;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogDebug("Fetching hut name for hutId: {HutId} - Attempt {Attempt}/{MaxAttempts}", hutId, attempt + 1, maxRetries + 1);
                using var httpClient = new HttpClient();
                var json = await httpClient.GetStringAsync($"https://www.hut-reservation.org/api/v1/reservation/hutInfo/{hutId}");

                var hutInfo = JsonSerializer.Deserialize(json, HutReservationJsonContext.Default.HutInformation)!;

                _logger.LogInformation("Successfully retrieved hut name '{HutName}' for hutId: {HutId}", hutInfo.HutName, hutId);
                return hutInfo.HutName;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden && attempt < maxRetries)
            {
                var delayMs = initialDelayMs * (int)Math.Pow(2, attempt);
                _logger.LogWarning(ex, "Transient HTTP 403 error when retrieving hut name for hutId: {HutId}. Retrying in {DelayMs}ms (Attempt {Attempt}/{MaxRetries})", hutId, delayMs, attempt + 1, maxRetries);
                await Task.Delay(delayMs);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                _logger.LogError(ex, "Failed to retrieve hut name. Hut with ID {HutId} not found (HTTP 400)", hutId);
                Console.Error.WriteLine($"Fehler beim Abrufen des Hüttennamens. Möglicherweise wurde die Hütte mit der ID {hutId} nicht gefunden.");
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed when retrieving hut name for hutId: {HutId}. Status Code: {StatusCode}", hutId, ex.StatusCode);
                Console.Error.WriteLine($"Fehler beim Abrufen des Hüttennamens für die Hütte mit der ID {hutId}.");
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization failed when retrieving hut information for hutId: {HutId}", hutId);
                Console.Error.WriteLine($"Fehler beim Abrufen der Hütteninformationen: {ex.Message}");
                return null;
            }
        }

        // If we reach here, all retries have been exhausted for 403 errors
        _logger.LogError("All retry attempts exhausted when retrieving hut name for hutId: {HutId}", hutId);
        Console.Error.WriteLine($"Fehler beim Abrufen des Hüttennamens für die Hütte mit der ID {hutId}. Alle Wiederholungsversuche fehlgeschlagen.");
        return null;
    }

    public async Task<IReadOnlyList<HutUsage>> GetUsages(Hut hut)
    {
        const int maxRetries = 3;
        const int initialDelayMs = 1000;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogDebug("Fetching usage data for hut '{HutName}' (ID: {HutId}) - Attempt {Attempt}/{MaxAttempts}", hut.Name, hut.Id, attempt + 1, maxRetries + 1);
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
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden && attempt < maxRetries)
            {
                var delayMs = initialDelayMs * (int)Math.Pow(2, attempt);
                _logger.LogWarning(ex, "Transient HTTP 403 error when retrieving usage data for hut '{HutName}' (ID: {HutId}). Retrying in {DelayMs}ms (Attempt {Attempt}/{MaxRetries})", hut.Name, hut.Id, delayMs, attempt + 1, maxRetries);
                await Task.Delay(delayMs);
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

        // If we reach here, all retries have been exhausted
        _logger.LogError("All retry attempts exhausted when retrieving usage data for hut '{HutName}' (ID: {HutId})", hut.Name, hut.Id);
        Console.Error.WriteLine($"Fehler beim Abrufen der Nutzungsdaten für die Hütte mit der ID {hut.Id}. Alle Wiederholungsversuche fehlgeschlagen.");
        return Array.Empty<HutUsage>();
    }
}