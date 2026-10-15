namespace RadioactiveDecay.Core.Models;

/// <summary>All time values must use the same unit.</summary>
public sealed record DecayParameters(
    double InitialQuantity,
    double HalfLife,
    double Duration,
    double Step);
