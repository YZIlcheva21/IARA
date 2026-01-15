using IARA.API.Data;
using IARA.API.Services;
using IARA.Domain.Models;
using IARA.Domain.Models.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization; // <--- ВАЖНО: Добавено за JSON настройките

var builder = WebApplication.CreateBuilder(args);

// Конфигурация
var configuration = builder.Configuration;

// Добавяне на услуги
// 👇 ПРОМЯНА: Конфигуриране на JSON сериализацията да игнорира циклични връзки
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();

// Swagger конфигурация
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IARA API",
        Version = "v1",
        Description = "API за Информационна Система на Изпълнителната Агенция по Рибарство и Аквакултури"
    });
});

// ИЗПОЛЗВАМЕ IN-MEMORY БАЗА ВМЕСТО SQL SERVER
builder.Services.AddDbContext<IARAContext>(options =>
    options.UseInMemoryDatabase("IARA_InMemoryDB"));

// Identity
builder.Services.AddIdentity<ApplicationUser, UserRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<IARAContext>()
.AddDefaultTokenProviders()
.AddRoles<UserRole>();

// JWT Authentication
var jwtKey = configuration["Jwt:Key"] ?? "YourSuperSecretKeyHereAtLeast32CharactersLong";
var jwtIssuer = configuration["Jwt:Issuer"] ?? "IARA.API";
var jwtAudience = configuration["Jwt:Audience"] ?? "IARA.Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Регистриране на ReportService
builder.Services.AddScoped<IReportService, ReportService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Конфигурация на пайплайна
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "IARA API v1");
        options.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Инициализация на базата данни със семпъл данни
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<IARAContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<UserRole>>();
        
        // Създаване на базата данни (в паметта)
        await context.Database.EnsureCreatedAsync();
        
        // Тъй като е In-Memory, тя винаги е празна при старт, затова я пълним:
        await SeedSampleData(context);
        
        // Създаване на роли и админ
        await InitializeRolesAsync(roleManager);
        await InitializeAdminAsync(userManager);
        
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Базата данни (In-Memory) е инициализирана успешно със семпъл данни");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Грешка при инициализация на базата данни");
    }
}

// Прост тест ендпойнт
app.MapGet("/", () => "IARA API is running (In-Memory)! Use Swagger at root for API documentation.");
app.MapGet("/test", () => new { 
    message = "API работи успешно!", 
    time = DateTime.Now,
    reports = new[] {
        "/api/Reports/expiring-licenses",
        "/api/Reports/amateur-ranking",
        "/api/Reports/ship-catch-analysis/2024",
        "/api/Reports/ship-fuel-efficiency/2024"
    }
});

app.Run();

// --- ПОМОЩНИ МЕТОДИ (Остават същите) ---

static async Task InitializeRolesAsync(RoleManager<UserRole> roleManager)
{
    string[] roles = { "Admin", "Inspector", "Fisher", "LicenseOfficer", "RegistryOfficer", "User" };
    
    foreach (var roleName in roles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new UserRole { Name = roleName });
        }
    }
}

static async Task InitializeAdminAsync(UserManager<ApplicationUser> userManager)
{
    var adminEmail = "admin@iara.bg";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        var admin = new ApplicationUser
        {
            UserName = "admin",
            Email = adminEmail,
            FirstName = "Администратор",
            LastName = "Система",
            EmailConfirmed = true
        };
        
        var createResult = await userManager.CreateAsync(admin, "Admin123!");
        
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
            await userManager.AddToRoleAsync(admin, "Inspector");
        }
    }
}

static async Task SeedSampleData(IARAContext context)
{
    // Проверка дали вече има данни (за всеки случай)
    if (context.Fishers.Any()) return;

    Console.WriteLine("📊 Добавяне на семпъл данни в паметта...");
    
    // 1. Рибари
    var fisher1 = new Fisher 
    { 
        FirstName = "Иван", LastName = "Петров", PersonalNumber = "8001011234",
        Email = "ivan.petrov@example.com", Phone = "+359888111222"
    };
    var fisher2 = new Fisher 
    { 
        FirstName = "Георги", LastName = "Иванов", PersonalNumber = "7505055678",
        Email = "georgi.ivanov@example.com", Phone = "+359888333444"
    };
    var fisher3 = new Fisher 
    { 
        FirstName = "Мария", LastName = "Димитрова", PersonalNumber = "9009109012",
        Email = "maria.dimitrova@example.com", Phone = "+359888555666"
    };
    context.Fishers.AddRange(fisher1, fisher2, fisher3);
    await context.SaveChangesAsync();
    
    // 2. Кораби
    var ship1 = new Ship 
    { 
        Name = "Посейдон", InternationalNumber = "IMO1234567", CallSign = "LZ1234", Marking = "PDN-001",
        RegistrationNumber = "BG-001", HomePort = "Варна", Length = 15.5m, Width = 4.2m, GrossTonnage = 45.8m,
        Draught = 2.1m, EnginePower = 250, EngineType = "Diesel", FuelType = "Diesel", AverageFuelConsumptionPerHour = 25,
        BuiltYear = new DateTime(2015, 1, 1), IsLargeShip = true, OwnerFisherId = fisher1.Id, CaptainFisherId = fisher1.Id, IsActive = true
    };
    var ship2 = new Ship 
    { 
        Name = "Нептун", InternationalNumber = "IMO7654321", CallSign = "LZ5678", Marking = "NPT-002",
        RegistrationNumber = "BG-002", HomePort = "Бургас", Length = 8.5m, Width = 2.8m, GrossTonnage = 18.3m,
        Draught = 1.5m, EnginePower = 120, EngineType = "Diesel", FuelType = "Diesel", AverageFuelConsumptionPerHour = 15,
        BuiltYear = new DateTime(2018, 1, 1), IsLargeShip = false, OwnerFisherId = fisher2.Id, CaptainFisherId = fisher2.Id, IsActive = true
    };
    context.Ships.AddRange(ship1, ship2);
    await context.SaveChangesAsync();
    
    // 3. Разрешителни
    var license1 = new License
    {
        LicenseNumber = "LIC-2024-001", FisherId = fisher1.Id, ShipId = ship1.Id,
        IssueDate = DateTime.Now.AddMonths(-6), ExpiryDate = DateTime.Now.AddDays(15),
        Status = "Active", LicenseType = "Професионален риболов", IssuingAuthority = "ИАРА Варна"
    };
    var license2 = new License
    {
        LicenseNumber = "LIC-2024-002", FisherId = fisher2.Id, ShipId = ship2.Id,
        IssueDate = DateTime.Now.AddMonths(-12), ExpiryDate = DateTime.Now.AddMonths(6),
        Status = "Active", LicenseType = "Професионален риболов", IssuingAuthority = "ИАРА Бургас"
    };
    context.Licenses.AddRange(license1, license2);
    await context.SaveChangesAsync();

    // 4. Билети за любители
    var amateurTicket = new AmateurTicket
    {
        FisherId = fisher3.Id, TicketNumber = "TICKET-2024-001",
        IssueDate = DateTime.Now.AddMonths(-3), ExpiryDate = DateTime.Now.AddMonths(9),
        Status = "Active", IssuingAuthority = "ИАРА Онлайн"
    };
    context.AmateurTickets.Add(amateurTicket);
    await context.SaveChangesAsync();

    // 5. Любителски улов
    var amateurCatch1 = new AmateurCatch { AmateurTicketId = amateurTicket.Id, CatchDate = DateTime.Now.AddMonths(-1), FishSpecies = "Каракуда", WeightKgs = 12.5m, Quantity = 3, FishingLocation = "Язовир Ивайловград", FishingMethod = "Въдица" };
    var amateurCatch2 = new AmateurCatch { AmateurTicketId = amateurTicket.Id, CatchDate = DateTime.Now.AddMonths(-2), FishSpecies = "Сардина", WeightKgs = 8.3m, Quantity = 25, FishingLocation = "Черно море - Поморие", FishingMethod = "Въдица" };
    context.AmateurCatches.AddRange(amateurCatch1, amateurCatch2);
    await context.SaveChangesAsync();

    // 6. Дневници
    var logbook1 = new LogbookEntry { LicenseId = license1.Id, FishingDate = DateTime.Now.AddMonths(-1), StartTime = new TimeSpan(6, 0, 0), EndTime = new TimeSpan(14, 0, 0), FishingArea = "Черно море - север, сектор 12", FuelConsumptionLiters = 200, DistanceTraveled = 85, WeatherConditions = "Слънчево" };
    var logbook2 = new LogbookEntry { LicenseId = license1.Id, FishingDate = DateTime.Now.AddMonths(-2), StartTime = new TimeSpan(5, 30, 0), EndTime = new TimeSpan(16, 0, 0), FishingArea = "Черно море - юг, сектор 8", FuelConsumptionLiters = 250, DistanceTraveled = 120, WeatherConditions = "Облачно" };
    var logbook3 = new LogbookEntry { LicenseId = license2.Id, FishingDate = DateTime.Now.AddMonths(-1), StartTime = new TimeSpan(7, 0, 0), EndTime = new TimeSpan(13, 30, 0), FishingArea = "Черно море - изток, сектор 5", FuelConsumptionLiters = 90, DistanceTraveled = 45, WeatherConditions = "Спокойно" };
    context.LogbookEntries.AddRange(logbook1, logbook2, logbook3);
    await context.SaveChangesAsync();

    // 7. Детайли за улов
    context.CatchDetails.AddRange(
        new CatchDetail { LogbookEntryId = logbook1.Id, FishSpecies = "Скумрия", WeightKgs = 150.5m, Quantity = 120, FishingGear = "Мрежа", Notes = "Качествен улов" },
        new CatchDetail { LogbookEntryId = logbook1.Id, FishSpecies = "Херинга", WeightKgs = 85.3m, Quantity = 95, FishingGear = "Мрежа", Notes = "Стандартен улов" },
        new CatchDetail { LogbookEntryId = logbook2.Id, FishSpecies = "Паламуд", WeightKgs = 180.3m, Quantity = 45, FishingGear = "Въдица", Notes = "Добър улов" },
        new CatchDetail { LogbookEntryId = logbook3.Id, FishSpecies = "Кефал", WeightKgs = 65.8m, Quantity = 38, FishingGear = "Мрежа", Notes = "Среден улов" }
    );
    await context.SaveChangesAsync();

    // 8. Инспекции
    var inspection1 = new Inspection
    {
        LicenseId = license1.Id, ShipId = ship1.Id, InspectorId = 1,
        InspectionDate = DateTime.Now.AddMonths(-1), InspectionType = "Рутинна проверка",
        Status = "Completed", Violations = "Няма", ActionsTaken = "Всичко е наред"
    };
    context.Inspections.Add(inspection1);
    await context.SaveChangesAsync();

    Console.WriteLine("🎉 Всички семпъл данни са добавени успешно в In-Memory базата!");
}