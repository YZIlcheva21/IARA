using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using IARA.Web;
using IARA.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 1. Първо регистрираме AuthService
builder.Services.AddScoped<AuthService>();

// 2. РЕШЕНИЕТО НА ПРОБЛЕМА:
// Вместо AddHttpClient, използваме AddScoped с "фабрика" (lambda function).
// Това прави същото - създава HttpClient, слага му адреса и го подава на твоя клас.
builder.Services.AddScoped<AuthenticatedHttpClient>(sp => 
{
    // Взимаме вече регистрирания AuthService
    var authService = sp.GetRequiredService<AuthService>();
    
    // Създаваме нов HttpClient ръчно и му задаваме адреса
    var httpClient = new HttpClient 
    { 
        BaseAddress = new Uri("http://localhost:5028") 
    };

    // Връщаме готовия AuthenticatedHttpClient, като свързваме двете
    return new AuthenticatedHttpClient(httpClient, authService);
});

// 3. Обикновен HttpClient (за други компоненти, които не ползват AuthenticatedHttpClient)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5028") });

// 4. Authorization
builder.Services.AddAuthorizationCore();

var app = builder.Build();

// Инициализация на auth
var authService = app.Services.GetRequiredService<AuthService>();
await authService.InitializeAsync();

await app.RunAsync();