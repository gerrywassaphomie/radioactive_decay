using RadioactiveDecay.Core.Models;

namespace RadioactiveDecay.Core.Services;

/// <summary>Analytical decay calculation; at most one million samples per request.</summary>
public sealed class RadioactiveDecayCalculator : IRadioactiveDecayCalculator
{
    public const int MaximumPointCount = 1_000_000;
    private const double RelativeTimeTolerance = 8 * 2.2204460492503131e-16;

    public IReadOnlyList<DecayPoint> Calculate(DecayParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        Validate(parameters.InitialQuantity, nameof(parameters.InitialQuantity), false);
        Validate(parameters.HalfLife, nameof(parameters.HalfLife), false);
        Validate(parameters.Duration, nameof(parameters.Duration), true);
        Validate(parameters.Step, nameof(parameters.Step), false);

        var lambda = Math.Log(2) / parameters.HalfLife;
        if (!double.IsFinite(lambda) || !double.IsFinite(lambda * parameters.InitialQuantity))
            throw new ArgumentOutOfRangeException(nameof(parameters), "Decay constant or initial activity exceeds double precision range.");

        var points = new List<DecayPoint>();
        DecayPoint At(double time)
        {
            var remaining = parameters.InitialQuantity * Math.Exp(-lambda * time);
            return new DecayPoint(time, remaining, lambda * remaining);
        }

        points.Add(At(0));
        if (parameters.Duration == 0)
            return points.AsReadOnly();

        var intervals = parameters.Duration / parameters.Step;
        if (!double.IsFinite(intervals) || Math.Ceiling(intervals) + 1 > MaximumPointCount)
            throw new ArgumentOutOfRangeException(nameof(parameters.Step), $"A calculation may contain at most {MaximumPointCount} points.");

        for (var index = 1; index <= (int)Math.Ceiling(intervals); index++)
        {
            var time = index * parameters.Step;
            if (time >= parameters.Duration ||
                parameters.Duration - time <= RelativeTimeTolerance * parameters.Duration)
                break;
            points.Add(At(time));
        }

        points.Add(At(parameters.Duration));
        return points.AsReadOnly();
    }

    private static void Validate(double value, string name, bool allowZero)
    {
        if (!double.IsFinite(value) || (allowZero ? value < 0 : value <= 0))
            throw new ArgumentOutOfRangeException(name, value, allowZero
                ? "Value must be finite and non-negative."
                : "Value must be finite and positive.");
    }
}
