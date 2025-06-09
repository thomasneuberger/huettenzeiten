using HuettenZeiten.Data.Models;

namespace HuettenZeiten.Data;

public interface IHutService
{
    Task<IReadOnlyList<HutUsage>> GetUsages(Hut hut);
}
