using HuettenZeiten.Data;
using HuettenZeiten.Data.Models;
using HuettenZeiten.Data.Services;
using HuettenZeiten.Output;
using HuettenZeiten.Output.Services;

var hut = new Hut { Id = 219, Name = "Simony-Hütte" };

IHutService hutService = new HutReservation();

var usages = await hutService.GetUsages(hut);

foreach (var usage in usages)
{
    if (usage.IsOpen)
    {
        Console.WriteLine($"{usage.Date}: {usage.FreeBeds} free beds");
    }
    else
    {
        Console.WriteLine($"{usage.Date}: closed");
    }
}

IOutputService output = new OutputHtml();
await output.Output(hut, usages);