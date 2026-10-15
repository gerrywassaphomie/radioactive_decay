using RadioactiveDecay.Core.Models;

namespace RadioactiveDecay.Core.Services;

public interface IRadioactiveDecayCalculator
{
    IReadOnlyList<DecayPoint> Calculate(DecayParameters parameters);
}
