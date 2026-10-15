namespace RadioactiveDecay.Core.Models;

/// <summary>Activity is expressed as decays per chosen time unit (Bq only for seconds).</summary>
public sealed record DecayPoint(double Time, double RemainingQuantity, double Activity);
