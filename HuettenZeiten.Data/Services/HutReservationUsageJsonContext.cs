using System.Text.Json;
using System.Text.Json.Serialization;

namespace HuettenZeiten.Data.Services;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, UseStringEnumConverter = true)]
[JsonSerializable(typeof(HutReservationUsage[]))]
internal partial class HutReservationUsageJsonContext : JsonSerializerContext
{
}