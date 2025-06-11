using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NpvCalculator.Shared.Services;
using NPVCalculator.Client;
using NPVCalculator.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrapProviders()
    .AddFontAwesomeIcons();

// Configure HttpClient with base address pointing to your API
var apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? "https://localhost:7191/";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseAddress) });

// Register your services
builder.Services.AddScoped<INpvService, NpvService>();

await builder.Build().RunAsync();