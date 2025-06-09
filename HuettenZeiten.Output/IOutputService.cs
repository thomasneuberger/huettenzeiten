using HuettenZeiten.Data.Models;

namespace HuettenZeiten.Output;

public interface IOutputService
{
    Task Output(Hut hut, IReadOnlyList<HutUsage> usages);
}
