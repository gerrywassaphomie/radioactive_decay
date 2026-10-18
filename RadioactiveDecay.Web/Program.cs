using RadioactiveDecay.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IRadioactiveDecayCalculator, RadioactiveDecayCalculator>();

var app = builder.Build();

app.UseStaticFiles();
app.MapDefaultControllerRoute();

app.Run();
