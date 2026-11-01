using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using StudentContingent.Client;
using StudentContingent.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Адрес API берётся из wwwroot/appsettings.json.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7080/";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddScoped<ContingentApi>();

await builder.Build().RunAsync();
