using HuettenZeiten.Data.Models;
using System.Text;

namespace HuettenZeiten.Output.Services;

public class OutputHtml : IOutputService
{
    private readonly DateOnly _from = DateOnly.FromDateTime(DateTime.Today);
    private readonly DateOnly _until = DateOnly.FromDateTime(DateTime.Today.AddYears(1));

    public async Task Output(IReadOnlyList<Tour> tours, IDictionary<int, IReadOnlyList<HutUsage>> usages)
    {
        var html = new StringBuilder();
        html.AppendLine("<html>");
        html.AppendLine("<body>");
        
        foreach (var tour in tours)
        {
            OutputTour(tour, usages, html);
        }

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        var file = Path.GetFullPath("usages.html");
        var content = html.ToString();
        await File.WriteAllTextAsync(file, content);

        Console.WriteLine($"Html file written to {file}");
    }

    private void OutputTour(Tour tour, IDictionary<int, IReadOnlyList<HutUsage>> usages, StringBuilder html)
    {
        html.AppendLine("<h2>").Append(tour.Name).AppendLine("</h2>");
        html.AppendLine("<table>");
        OutputTableHeader(html);
        foreach (var hut in tour.Huts)
        {
            if (usages.TryGetValue(hut.Id, out var hutUsages))
            {
                OutputHut(hut, hutUsages, html);
            }
            else
            {
                html.AppendLine("<tr><td colspan='100%'>Keine Daten für diese Hütte verfügbar</td></tr>");
            }
        }
        html.AppendLine("</table>");
    }

    private void OutputTableHeader(StringBuilder html)
    {
        html.AppendLine("<thead>");
        html.AppendLine("<tr>");
        html.Append("<th>").Append("Hütte").AppendLine("</th>");
        for (var date = _from; date <= _until; date = date.AddDays(1))
        {
            html.Append("<th>").Append(date.ToString("dddd")).AppendLine("</th>");
        }
        html.AppendLine("</tr>");
        html.AppendLine("<tr>");
        html.Append("<th></th>");
        for (var date = _from; date <= _until; date = date.AddDays(1))
        {
            html.Append("<th>").Append(date.ToString()).AppendLine("</th>");
        }
        html.AppendLine("</tr>");
        html.AppendLine("</thead>");
    }

    private void OutputHut(Hut hut, IReadOnlyList<HutUsage> usages, StringBuilder html)
    {
        html.AppendLine("<tr>");
        html.Append("<td>").Append(hut.Name).AppendLine("</td>");
        for (var date = _from; date <= _until; date = date.AddDays(1))
        {
            html.Append("<td>");
            var usage = usages.FirstOrDefault(u => u.Date == date);
            if (usage?.IsOpen == true)
            {
                html.Append(usage.FreeBeds);
            }
            html.AppendLine("</td>");
        }
        html.AppendLine("</tr>");
    }
}
