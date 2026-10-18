using System.Text.Json;
using RadioactiveDecay.Core.Models;

namespace RadioactiveDecay.Export;

public sealed class JsonDecayResultExporter : IDecayResultExporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public string Format => "json";
    public string ContentType => "application/json; charset=utf-8";

    public byte[] Export(DecayParameters parameters, IReadOnlyList<DecayPoint> points)
    {
        ExportValidation.Validate(parameters, points);
        return JsonSerializer.SerializeToUtf8Bytes(new { parameters, points }, Options);
    }
}
