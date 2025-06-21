using System.Text.Json;
using System.Text.Json.Serialization;
using HuettenZeiten.Data.Models;
using HuettenZeiten.Data.Services;

namespace HuettenZeiten.Data;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, UseStringEnumConverter = true)]
[JsonSerializable(typeof(HutReservationUsage[]))]
[JsonSerializable(typeof(HutInformation))]
[JsonSerializable(typeof(Hut))]
[JsonSerializable(typeof(Tour[]))]
internal partial class HutReservationJsonContext : JsonSerializerContext
{
}