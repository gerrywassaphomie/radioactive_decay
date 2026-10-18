using RadioactiveDecay.Core.Models;

namespace RadioactiveDecay.Export;

internal static class ExportValidation
{
    public static void Validate(DecayParameters parameters, IReadOnlyList<DecayPoint> points)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(points);
        if (!double.IsFinite(parameters.InitialQuantity) || !double.IsFinite(parameters.HalfLife) ||
            !double.IsFinite(parameters.Duration) || !double.IsFinite(parameters.Step))
            throw new ArgumentException("Parameters must be finite.", nameof(parameters));
        foreach (var point in points)
        {
            if (point is null || !double.IsFinite(point.Time) ||
                !double.IsFinite(point.RemainingQuantity) || !double.IsFinite(point.Activity))
                throw new ArgumentException("Points must be non-null and finite.", nameof(points));
        }
    }
}
