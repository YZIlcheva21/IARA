using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using IARA.Web;
using IARA.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient за връзка с API-то
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5028") });

// Authentication service
builder.Services.AddScoped<AuthService>();

// Authorization
builder.Services.AddAuthorizationCore();

var app = builder.Build();

// Initialize authentication
var authService = app.Services.GetRequiredService<AuthService>();
await authService.InitializeAsync();

await app.RunAsync();