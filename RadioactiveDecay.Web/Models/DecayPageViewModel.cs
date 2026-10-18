using System.ComponentModel.DataAnnotations;
using RadioactiveDecay.Core.Models;

namespace RadioactiveDecay.Web.Models;

public sealed class DecayPageViewModel : IValidatableObject
{
    public const int MaximumWebPoints = 2000;

    [Display(Name = "Начальное количество, ядер")]
    [Required(ErrorMessage = "Введите начальное количество.")]
    public string InitialQuantity { get; set; } = "1000";

    [Display(Name = "Период полураспада, с")]
    [Required(ErrorMessage = "Введите период полураспада.")]
    public string HalfLife { get; set; } = "10";

    [Display(Name = "Продолжительность, с")]
    [Required(ErrorMessage = "Введите продолжительность.")]
    public string Duration { get; set; } = "50";

    [Display(Name = "Шаг расчёта, с")]
    [Required(ErrorMessage = "Введите шаг расчёта.")]
    public string Step { get; set; } = "1";

    public IReadOnlyList<DecayPoint> Points { get; set; } = Array.Empty<DecayPoint>();

    // Explicit parsing accepts either decimal separator, without ambiguous thousands separators.
    private static bool TryNumber(string? text, out double value) =>
        double.TryParse(text?.Trim().Replace(',', '.'),
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out value) && double.IsFinite(value);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var fields = new[] {
            (nameof(InitialQuantity), InitialQuantity, false),
            (nameof(HalfLife), HalfLife, false),
            (nameof(Duration), Duration, true),
            (nameof(Step), Step, false)
        };
        bool valid = true;
        foreach (var (name, text, allowZero) in fields)
        {
            if (!TryNumber(text, out var number) || (allowZero ? number < 0 : number <= 0))
            {
                valid = false;
                yield return new ValidationResult(allowZero
                    ? "Введите конечное число, не меньше нуля."
                    : "Введите конечное число, больше нуля.", new[] { name });
            }
        }
        if (valid)
        {
            var p = ToParameters();
            if (p.Duration > 0 && (!double.IsFinite(p.Duration / p.Step) ||
                Math.Ceiling(p.Duration / p.Step) + 1 > MaximumWebPoints))
                yield return new ValidationResult(
                    "На странице допускается до 2 000 точек. Увеличьте шаг или уменьшите продолжительность.",
                    new[] { nameof(Step) });
        }
    }

    public DecayParameters ToParameters()
    {
        TryNumber(InitialQuantity, out var initial);
        TryNumber(HalfLife, out var halfLife);
        TryNumber(Duration, out var duration);
        TryNumber(Step, out var step);
        return new(initial, halfLife, duration, step);
    }
}

