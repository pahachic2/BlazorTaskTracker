using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TaskTracker.Web.Components;
using TaskTracker.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

if (builder.HostEnvironment.IsDevelopment())
{
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5109/") });
}
else
{
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://tasktracker.graff.tech/") });
}

// Регистрируем сервис для работы с localStorage
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

// Регистрируем сервис для работы с API
builder.Services.AddScoped<IApiService, ApiService>();

await builder.Build().RunAsync();
