using HuettenZeiten.Cli.Logging;
using HuettenZeiten.Cli.Prompts;
using HuettenZeiten.Data;
using HuettenZeiten.Data.Models;
using HuettenZeiten.Data.Prompts;
using HuettenZeiten.Data.Services;
using HuettenZeiten.Data.Storage;
using HuettenZeiten.Output;
using HuettenZeiten.Output.Services;
using Microsoft.Extensions.Logging;
using Sharprompt;

// Set up logging
var logFilePath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "HuettenZeiten",
    "app.log");

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddProvider(new FileLoggerProvider(logFilePath));
    builder.SetMinimumLevel(LogLevel.Information);
});

var logger = loggerFactory.CreateLogger<Program>();
logger.LogInformation("Application started");

ITourStorage tourStorage = new TourStorage();

var tours = await tourStorage.LoadTours();
logger.LogInformation("Loaded {TourCount} tours", tours.Count);

if (tours.Count == 0)
{
    tours = [
        new(){
            Id = 1,
            Name = "Standard",
            Huts = [
                new Hut { Id = 219, Name = "Simony-Hütte" }
            ]
        }
    ];
}

var action = Prompt.Select<MainActions>("Was möchtest du tun?");
logger.LogInformation("User selected action: {Action}", action);
switch (action)
{
    case MainActions.ManageHuts:
        await ManageHuts(tours, tourStorage, loggerFactory);
        break;
    case MainActions.OutputUsages:
        await OutputUsages(tours, loggerFactory);
        break;
    case MainActions.Quit:
        logger.LogInformation("Application exiting");
        return;
    default:
        Console.WriteLine("Unbekannte Aktion.");
        logger.LogWarning("Unknown action selected");
        return;
}

async Task ManageHuts(IReadOnlyList<Tour> tours, ITourStorage tourStorage, ILoggerFactory loggerFactory)
{
    logger.LogInformation("Entered ManageHuts mode");
    var hutReservation = new HutReservation(loggerFactory.CreateLogger<HutReservation>());

    var selectedTour = tours.Count == 1
        ? tours[0]
        : SelectTour(tours);

    while (true)
    {
        PrintTour(selectedTour);

        var action = Prompt.Select<ManageHutsActions>("Was möchtest du tun?");
        switch (action)
        {
            case ManageHutsActions.AddHut:
                var hutId = Prompt.Input<int>("Bitte gib die ID der Hütte ein:");
                logger.LogInformation("User attempting to add hut with ID: {HutId}", hutId);
                if (selectedTour.Huts.Any(h => h.Id == hutId))
                {
                    Console.WriteLine("Diese Hütte ist bereits in der Tour enthalten.");
                    logger.LogWarning("Hut with ID {HutId} already exists in tour", hutId);
                    continue;
                }

                var hutName = await hutReservation.GetHutName(hutId);
                if (string.IsNullOrWhiteSpace(hutName))
                {
                    logger.LogWarning("Could not retrieve hut name for ID {HutId}", hutId);
                    continue;
                }
                if (selectedTour.Huts.Any(h => h.Name == hutName))
                {
                    hutName += $" (ID: {hutId})";
                }
                else
                {
                    selectedTour.Huts.Add(new Hut { Id = hutId, Name = hutName });
                    await tourStorage.SaveTour(selectedTour);
                    Console.WriteLine($"Hütte '{hutName}' (ID: {hutId}) wurde hinzugefügt.");
                    logger.LogInformation("Added hut '{HutName}' (ID: {HutId}) to tour '{TourName}'", hutName, hutId, selectedTour.Name);
                }
                break;
            case ManageHutsActions.RemoveHut:
                if (selectedTour.Huts.Count == 0)
                {
                    Console.WriteLine("Keine Hütten zum Entfernen vorhanden.");
                    logger.LogInformation("No huts available to remove");
                    continue;
                }
                var hutToRemove = Prompt.Select("Bitte wähle eine Hütte zum Entfernen:", selectedTour.Huts.Select(h => h.Name).Concat(["Keine"]).ToArray());
                var hut = selectedTour.Huts.FirstOrDefault(h => h.Name == hutToRemove);
                if (hut == null)
                {
                    continue;
                }

                selectedTour.Huts.Remove(hut);
                await tourStorage.SaveTour(selectedTour);

                Console.WriteLine($"Hütte '{hut.Name}' (ID: {hut.Id}) wurde entfernt.");
                logger.LogInformation("Removed hut '{HutName}' (ID: {HutId}) from tour '{TourName}'", hut.Name, hut.Id, selectedTour.Name);
                break;
            case ManageHutsActions.Finished:
                logger.LogInformation("Exiting ManageHuts mode");
                return;
            default:
                Console.WriteLine("Unbekannte Aktion.");
                continue;
        }
    }
}


// Method to select a tour from a list using Sharprompt
Tour SelectTour(IReadOnlyList<Tour> tours)
{
    var selectedName = Prompt.Select("Bitte wähle eine Tour aus: ", tours.Select(t => t.Name).ToArray());
    var selectedTour = tours.First(t => t.Name == selectedName);
    logger.LogInformation("User selected tour: {TourName}", selectedTour.Name);
    return selectedTour;
}

static async Task OutputUsages(IReadOnlyList<Tour> tours, ILoggerFactory loggerFactory)
{
    var logger = loggerFactory.CreateLogger("OutputUsages");

    logger.LogInformation("Starting to retrieve usages for {TourCount} tours", tours.Count);
    IHutService hutService = new HutReservation(loggerFactory.CreateLogger<HutReservation>());

    // Dictionary to store usages for each hut by hut id
    var hutUsages = new Dictionary<int, IReadOnlyList<HutUsage>>();

    foreach (var tour in tours)
    {
        foreach (var hut in tour.Huts)
        {
            try
            {
                logger.LogInformation("Fetching usages for hut '{HutName}' (ID: {HutId})", hut.Name, hut.Id);
                IReadOnlyList<HutUsage> usages = await hutService.GetUsages(hut);
                hutUsages[hut.Id] = usages;
                logger.LogInformation("Retrieved {UsageCount} usage entries for hut '{HutName}'", usages.Count, hut.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching usages for hut '{HutName}' (ID: {HutId})", hut.Name, hut.Id);
                Console.Error.WriteLine($"Fehler beim Abrufen der Daten für Hütte '{hut.Name}' (ID: {hut.Id}). Die Hütte wird übersprungen.");
                hutUsages[hut.Id] = Array.Empty<HutUsage>();
            }
        }
    }

    IOutputService output = new OutputHtml();
    logger.LogInformation("Generating output");
    await output.Output(tours, hutUsages);
    logger.LogInformation("Output generation completed");
}

static void PrintTour(Tour selectedTour)
{
    Console.WriteLine("Hütten:");

    if (selectedTour.Huts.Count == 0)
    {
        Console.WriteLine("Keine Hütten in dieser Tour.");
        return;
    }

    foreach (var hut in selectedTour.Huts)
    {
        Console.WriteLine($"- {hut.Name} (ID: {hut.Id})");
    }
}