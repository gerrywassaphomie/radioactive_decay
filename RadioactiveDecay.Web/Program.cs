using RadioactiveDecay.Core.Services;
using RadioactiveDecay.Export;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IRadioactiveDecayCalculator, RadioactiveDecayCalculator>();
builder.Services.AddSingleton<IDecayResultExporter, CsvDecayResultExporter>();
builder.Services.AddSingleton<IDecayResultExporter, JsonDecayResultExporter>();

var app = builder.Build();

app.UseStaticFiles();
app.MapDefaultControllerRoute();

app.Run();
