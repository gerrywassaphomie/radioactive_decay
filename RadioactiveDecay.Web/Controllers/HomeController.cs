using Microsoft.AspNetCore.Mvc;
using RadioactiveDecay.Core.Services;
using RadioactiveDecay.Export;
using RadioactiveDecay.Web.Models;

namespace RadioactiveDecay.Web.Controllers;

public sealed class HomeController(IRadioactiveDecayCalculator calculator,
    IEnumerable<IDecayResultExporter> exporters) : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new DecayPageViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(DecayPageViewModel model)
    {
        Calculate(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Download(DecayPageViewModel model, string format)
    {
        var exporter = exporters.FirstOrDefault(x => x.Format == format);
        if (exporter is null)
            ModelState.AddModelError(string.Empty, "Выберите формат CSV или JSON.");
        if (!Calculate(model)) return View("Index", model);
        return File(exporter!.Export(model.ToParameters(), model.Points),
            exporter.ContentType, $"radioactive-decay.{exporter.Format}");
    }

    private bool Calculate(DecayPageViewModel model)
    {
        if (!ModelState.IsValid) return false;
        try
        {
            model.Points = calculator.Calculate(model.ToParameters());
        }
        catch (ArgumentException)
        {
            ModelState.AddModelError(string.Empty,
                "Эти параметры выходят за допустимый диапазон расчёта. Уменьшите начальное количество или увеличьте период полураспада и шаг.");
        }
        return ModelState.IsValid;
    }
}

