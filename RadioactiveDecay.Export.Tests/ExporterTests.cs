using System.Globalization;
using System.Text;
using System.Text.Json;
using RadioactiveDecay.Core.Models;
using RadioactiveDecay.Core.Services;

namespace RadioactiveDecay.Export.Tests;

public sealed class ExporterTests
{
    private static readonly DecayParameters Parameters = new(1000, 10, 50, 1);
    private static IReadOnlyList<DecayPoint> Points => new RadioactiveDecayCalculator().Calculate(Parameters);
    private static IDecayResultExporter Exporter(string format) =>
        format == "csv" ? new CsvDecayResultExporter() : new JsonDecayResultExporter();

    [Fact]
    public void CsvContainsHeaderAndEveryPoint()
    {
        var rows = Encoding.UTF8.GetString(new CsvDecayResultExporter().Export(Parameters, Points))
            .Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal("TimeSeconds,RemainingQuantity,ActivityBq", rows[0]);
        Assert.Equal(Points.Count + 1, rows.Length);
        for (var i = 0; i < Points.Count; i++)
        {
            var cells = rows[i + 1].Split(',');
            Assert.Equal(Points[i].Time, double.Parse(cells[0], CultureInfo.InvariantCulture));
            Assert.Equal(Points[i].RemainingQuantity, double.Parse(cells[1], CultureInfo.InvariantCulture));
            Assert.Equal(Points[i].Activity, double.Parse(cells[2], CultureInfo.InvariantCulture));
        }
    }

    [Theory]
    [InlineData("ru-RU")]
    [InlineData("fr-FR")]
    public void CsvIsIndependentOfCulture(string culture)
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            var expected = new CsvDecayResultExporter().Export(Parameters, Points);
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            Assert.Equal(expected, new CsvDecayResultExporter().Export(Parameters, Points));
        }
        finally { CultureInfo.CurrentCulture = original; }
    }

    [Fact]
    public void JsonRoundTripsParametersAndEveryPoint()
    {
        using var document = JsonDocument.Parse(new JsonDecayResultExporter().Export(Parameters, Points));
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        Assert.Equal(Parameters, document.RootElement.GetProperty("parameters").Deserialize<DecayParameters>(options));
        Assert.Equal(Points.ToArray(), document.RootElement.GetProperty("points").Deserialize<DecayPoint[]>(options));
        Assert.Equal(JsonValueKind.Number, document.RootElement.GetProperty("parameters").GetProperty("halfLife").ValueKind);
    }

    [Theory]
    [InlineData("csv")]
    [InlineData("json")]
    public void ZeroDurationExportsOnePoint(string format)
    {
        var parameters = Parameters with { Duration = 0 };
        var points = new RadioactiveDecayCalculator().Calculate(parameters);
        var bytes = Exporter(format).Export(parameters, points);
        if (format == "json")
        {
            using var doc = JsonDocument.Parse(bytes);
            Assert.Equal(1, doc.RootElement.GetProperty("points").GetArrayLength());
        }
        else Assert.Equal(2, Encoding.UTF8.GetString(bytes).Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Length);
    }

    [Theory]
    [InlineData("csv")]
    [InlineData("json")]
    public void EmptyPointsProduceValidDocument(string format)
    {
        var bytes = Exporter(format).Export(Parameters, Array.Empty<DecayPoint>());
        if (format == "json")
        {
            using var doc = JsonDocument.Parse(bytes);
            Assert.Equal(0, doc.RootElement.GetProperty("points").GetArrayLength());
        }
        else Assert.Equal("TimeSeconds,RemainingQuantity,ActivityBq\r\n", Encoding.UTF8.GetString(bytes));
    }

    [Theory]
    [InlineData("csv")]
    [InlineData("json")]
    public void NullArgumentsAreRejected(string format)
    {
        Assert.Throws<ArgumentNullException>(() => Exporter(format).Export(null!, Points));
        Assert.Throws<ArgumentNullException>(() => Exporter(format).Export(Parameters, null!));
    }

    [Theory]
    [InlineData("csv", double.NaN)]
    [InlineData("csv", double.PositiveInfinity)]
    [InlineData("csv", double.NegativeInfinity)]
    [InlineData("json", double.NaN)]
    [InlineData("json", double.PositiveInfinity)]
    [InlineData("json", double.NegativeInfinity)]
    public void NonFiniteValuesAreRejected(string format, double value)
    {
        Assert.Throws<ArgumentException>(() => Exporter(format).Export(Parameters with { HalfLife = value }, Points));
        Assert.Throws<ArgumentException>(() => Exporter(format).Export(Parameters, new[] { new DecayPoint(0, value, 1) }));
    }

    [Theory]
    [InlineData("csv")]
    [InlineData("json")]
    public void NullPointIsRejected(string format) =>
        Assert.Throws<ArgumentException>(() => Exporter(format).Export(Parameters, new DecayPoint[] { null! }));
}
