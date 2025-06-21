using System.ComponentModel.DataAnnotations;

namespace HuettenZeiten.Data.Models;

public class Hut
{
    public int Id { get; set; }

    public required string Name { get; set; }
}
