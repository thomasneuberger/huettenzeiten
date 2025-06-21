using System.Text.Json;
using HuettenZeiten.Data.Models;

namespace HuettenZeiten.Data.Storage;

public class TourStorage : ITourStorage
{
    // Path to the tours.json file in the user's AppData/HuettenZeiten directory
    private readonly string _filePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "HuettenZeiten",
        "tours.json");

    /// <summary>
    /// Loads all tours from the tours.json file. Returns an empty list if the file does not exist or is invalid.
    /// </summary>
    /// <returns>A read-only list of Tour objects.</returns>
    public async Task<IReadOnlyList<Tour>> LoadTours()
    {
        if (!File.Exists(_filePath))
        {
            return Array.Empty<Tour>();
        }

        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize(json, HutReservationJsonContext.Default.TourArray) ?? Array.Empty<Tour>();
    }

    /// <summary>
    /// Saves a tour to the tours.json file. If a tour with the same Id exists, it is replaced at the same position to keep order.
    /// Otherwise, the new tour is added to the end of the list.
    /// </summary>
    /// <param name="tour">The Tour object to save.</param>
    public async Task SaveTour(Tour tour)
    {
        var tours = (await LoadTours()).ToList();

        // Find the index of the existing tour (by Id)
        var index = tours.FindIndex(t => t.Id == tour.Id);

        if (index >= 0)
        {
            // Replace at the same position to keep order
            tours[index] = tour;
        }
        else
        {
            // Add new tour if not found
            tours.Add(tour);
        }

        var json = JsonSerializer.Serialize(tours.ToArray(), HutReservationJsonContext.Default.TourArray);
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath) ?? string.Empty);
        await File.WriteAllTextAsync(_filePath, json);
    }
}
