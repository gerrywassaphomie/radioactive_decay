using Microsoft.AspNetCore.Mvc;
using RadioactiveDecay.Core.Services;
using RadioactiveDecay.Web.Models;

namespace RadioactiveDecay.Web.Controllers;

public sealed class HomeController(IRadioactiveDecayCalculator calculator) : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new DecayPageViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(DecayPageViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            model.Points = calculator.Calculate(model.ToParameters());
        }
        catch (ArgumentException)
        {
            ModelState.AddModelError(string.Empty,
                "Эти параметры выходят за допустимый диапазон расчёта. Уменьшите начальное количество или увеличьте период полураспада и шаг.");
        }
        return View(model);
    }
}

