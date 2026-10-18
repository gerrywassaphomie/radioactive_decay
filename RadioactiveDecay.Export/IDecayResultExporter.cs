using RadioactiveDecay.Core.Models;

namespace RadioactiveDecay.Export;

public interface IDecayResultExporter
{
    string Format { get; }
    string ContentType { get; }
    byte[] Export(DecayParameters parameters, IReadOnlyList<DecayPoint> points);
}
