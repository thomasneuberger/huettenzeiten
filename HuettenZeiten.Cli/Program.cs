using HuettenZeiten.Data;
using HuettenZeiten.Data.Models;
using HuettenZeiten.Data.Services;
using HuettenZeiten.Data.Storage;
using HuettenZeiten.Output;
using HuettenZeiten.Output.Services;

ITourStorage tourStorage = new TourStorage();

var tours = tourStorage.LoadTours();

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