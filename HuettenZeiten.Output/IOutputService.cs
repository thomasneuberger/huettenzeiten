using HuettenZeiten.Data.Models;

namespace HuettenZeiten.Output;

public interface IOutputService
{
    Task Output(IReadOnlyList<Tour> tours, IDictionary<int, IReadOnlyList<HutUsage>> usages, IDictionary<int, string> errors);
}
