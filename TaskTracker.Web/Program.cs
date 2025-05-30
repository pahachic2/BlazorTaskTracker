using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TaskTracker.Web.Components;

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

await builder.Build().RunAsync();
