namespace HuettenZeiten.Data.Models;

public class Tour
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required IList<Hut> Huts { get; set; }
}
