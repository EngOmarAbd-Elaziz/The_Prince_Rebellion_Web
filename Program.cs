using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AdventureGameWeb;
using AdventureGameWeb.Engine;
using AdventureGameWeb.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register AAA Game Engine & Presentation Services
builder.Services.AddScoped<AudioService>();
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<AchievementService>();
builder.Services.AddScoped<StatsService>();
builder.Services.AddScoped<UiStateService>();
builder.Services.AddScoped<SupabaseLeaderboardService>();
builder.Services.AddScoped<IGameEngineService, GameEngineService>();

await builder.Build().RunAsync();
