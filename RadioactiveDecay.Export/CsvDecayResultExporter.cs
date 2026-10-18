using System.Globalization;
using System.Text;
using RadioactiveDecay.Core.Models;

namespace RadioactiveDecay.Export;

public sealed class CsvDecayResultExporter : IDecayResultExporter
{
    public string Format => "csv";
    public string ContentType => "text/csv; charset=utf-8";

    public byte[] Export(DecayParameters parameters, IReadOnlyList<DecayPoint> points)
    {
        ExportValidation.Validate(parameters, points);
        var csv = new StringBuilder("TimeSeconds,RemainingQuantity,ActivityBq\r\n");
        foreach (var point in points)
        {
            csv.Append(point.Time.ToString("R", CultureInfo.InvariantCulture)).Append(',')
                .Append(point.RemainingQuantity.ToString("R", CultureInfo.InvariantCulture)).Append(',')
                .Append(point.Activity.ToString("R", CultureInfo.InvariantCulture)).Append("\r\n");
        }
        return Encoding.UTF8.GetBytes(csv.ToString());
    }
}
