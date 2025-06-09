using HuettenZeiten.Data.Models;
using System.Text;

namespace HuettenZeiten.Output.Services;

public class OutputHtml : IOutputService
{
    private readonly DateOnly _from = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
    private readonly DateOnly _until = new DateOnly(DateTime.Now.Year, 12, 31);

    public async Task Output(Hut hut, IReadOnlyList<HutUsage> usages)
    {
        var html = new StringBuilder();
        html.AppendLine("<html>");
        html.AppendLine("<body>");
        html.AppendLine("<table>");
        OutputTableHeader(html);
        OutputHut(hut, usages, html);
        html.AppendLine("</table>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        var file = Path.GetFullPath("usages.html");
        var content = html.ToString();
        await File.WriteAllTextAsync(file, content);

        Console.WriteLine($"Html file written to {file}");
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
