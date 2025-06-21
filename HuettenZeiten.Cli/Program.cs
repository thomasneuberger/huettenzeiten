using HuettenZeiten.Cli.Prompts;
using HuettenZeiten.Data;
using HuettenZeiten.Data.Models;
using HuettenZeiten.Data.Prompts;
using HuettenZeiten.Data.Services;
using HuettenZeiten.Data.Storage;
using HuettenZeiten.Output;
using HuettenZeiten.Output.Services;
using Sharprompt;

ITourStorage tourStorage = new TourStorage();

var tours = await tourStorage.LoadTours();

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
switch (action)
{
    case MainActions.ManageHuts:
        await ManageHuts(tours, tourStorage);
        break;
    case MainActions.OutputUsages:
        await OutputUsages(tours);
        break;
    case MainActions.Quit:
        return;
    default:
        Console.WriteLine("Unbekannte Aktion.");
        return;
}

async Task ManageHuts(IReadOnlyList<Tour> tours, ITourStorage tourStorage)
{
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
                if (selectedTour.Huts.Any(h => h.Id == hutId))
                {
                    Console.WriteLine("Diese Hütte ist bereits in der Tour enthalten.");
                    continue;
                }

                var hutName = await new HutReservation().GetHutName(hutId);
                if (string.IsNullOrWhiteSpace(hutName))
                {
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
                }
                break;
            case ManageHutsActions.RemoveHut:
                if (selectedTour.Huts.Count == 0)
                {
                    Console.WriteLine("Keine Hütten zum Entfernen vorhanden.");
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
                break;
            case ManageHutsActions.Finished:
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
    return tours.First(t => t.Name == selectedName);
}

static async Task OutputUsages(IReadOnlyList<Tour> tours)
{
    IHutService hutService = new HutReservation();

    // Dictionary to store usages for each hut by hut id
    var hutUsages = new Dictionary<int, IReadOnlyList<HutUsage>>();

    foreach (var tour in tours)
    {
        foreach (var hut in tour.Huts)
        {
            IReadOnlyList<HutUsage> usages = await hutService.GetUsages(hut);
            hutUsages[hut.Id] = usages;
        }
    }

    IOutputService output = new OutputHtml();
    await output.Output(tours, hutUsages);
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