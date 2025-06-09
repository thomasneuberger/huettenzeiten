namespace HuettenZeiten.Data.Models;

public class HutUsage
{
    public required bool IsOpen { get; set; }

    public required DateOnly Date { get; set; }

    public required int FreeBeds { get; set; }
}
