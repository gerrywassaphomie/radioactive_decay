using RadioactiveDecay.Core.Models;
using RadioactiveDecay.Core.Services;

namespace RadioactiveDecay.Core.Tests;

public sealed class RadioactiveDecayCalculatorTests
{
    private readonly IRadioactiveDecayCalculator calculator = new RadioactiveDecayCalculator();
    private static DecayParameters Valid => new(1000, 10, 20, 5);

    [Fact]
    public void InitialPointHasOriginalQuantity()
    {
        var point = calculator.Calculate(Valid)[0];
        Assert.Equal(0, point.Time);
        Assert.Equal(1000, point.RemainingQuantity);
    }

    [Theory]
    [InlineData(10, 500)]
    [InlineData(20, 250)]
    public void HalfLivesMatchReferenceValues(double time, double expected)
    {
        var point = calculator.Calculate(Valid with { Duration = time })[^1];
        Assert.InRange(Math.Abs(point.RemainingQuantity - expected), 0, 1e-10);
    }

    [Fact]
    public void ActivityMatchesDecayConstantTimesQuantity()
    {
        foreach (var point in calculator.Calculate(Valid))
            Assert.InRange(Math.Abs(point.Activity - Math.Log(2) / 10 * point.RemainingQuantity), 0, 1e-10);
    }

    [Fact]
    public void TimesIncreaseAndQuantitiesDecrease()
    {
        var points = calculator.Calculate(Valid);
        for (var i = 1; i < points.Count; i++)
        {
            Assert.True(points[i].Time > points[i - 1].Time);
            Assert.True(points[i].RemainingQuantity < points[i - 1].RemainingQuantity);
        }
    }

    [Theory]
    [InlineData(20, 5, 5)]
    [InlineData(21, 5, 6)]
    [InlineData(2, 5, 2)]
    [InlineData(0.3, 0.1, 4)]
    [InlineData(0.6, 0.2, 4)]
    [InlineData(1e-20, 1e-20, 2)]
    public void GridIncludesExactEndpointOnce(double duration, double step, int count)
    {
        var points = calculator.Calculate(Valid with { Duration = duration, Step = step });
        Assert.Equal(count, points.Count);
        Assert.Equal(duration, points[^1].Time);
        Assert.Single(points, point => point.Time == duration);
        Assert.Equal(0, points[0].Time);
        for (var i = 1; i < points.Count; i++)
            Assert.True(points[i].Time > points[i - 1].Time);
    }

    [Fact]
    public void ZeroDurationReturnsOnlyInitialPoint()
    {
        var point = Assert.Single(calculator.Calculate(Valid with { Duration = 0 }));
        Assert.Equal(0, point.Time);
        Assert.Equal(1000, point.RemainingQuantity);
    }

    public static IEnumerable<object[]> InvalidValues()
    {
        foreach (var field in new[] { "InitialQuantity", "HalfLife", "Duration", "Step" })
        {
            foreach (var value in new[] { -1d, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
                yield return new object[] { field, value };
            if (field != "Duration")
                yield return new object[] { field, 0d };
        }
    }

    [Theory]
    [MemberData(nameof(InvalidValues))]
    public void InvalidInputIsRejected(string field, double value)
    {
        var parameters = field switch
        {
            "InitialQuantity" => Valid with { InitialQuantity = value },
            "HalfLife" => Valid with { HalfLife = value },
            "Duration" => Valid with { Duration = value },
            _ => Valid with { Step = value }
        };
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => calculator.Calculate(parameters));
        Assert.Equal(field, exception.ParamName);
    }

    [Fact]
    public void NullIsRejected() =>
        Assert.Throws<ArgumentNullException>(() => calculator.Calculate(null!));

    [Fact]
    public void ExcessiveGridIsRejected() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => calculator.Calculate(Valid with { Step = double.Epsilon }));

    [Fact]
    public void UnrepresentableDecayConstantIsRejected() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => calculator.Calculate(Valid with { HalfLife = double.Epsilon }));
}
