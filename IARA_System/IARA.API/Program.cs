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

var builder = WebApplication.CreateBuilder(args);

// Конфигурация
var configuration = builder.Configuration;

// Добавяне на услуги
builder.Services.AddControllers();
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

// База данни
var connectionString = configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=(localdb)\\mssqllocaldb;Database=IARA_DB;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<IARAContext>(options =>
    options.UseSqlServer(connectionString));

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
        
        // Създаване на базата данни
        await context.Database.EnsureCreatedAsync();
        
        // Добавяне на семпъл данни само ако базата е празна
        if (!context.Fishers.Any())
        {
            await SeedSampleData(context);
        }
        
        // Създаване на роли
        await InitializeRolesAsync(roleManager);
        
        // Създаване на администратор
        await InitializeAdminAsync(userManager);
        
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Базата данни е инициализирана успешно със семпъл данни");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Грешка при инициализация на базата данни");
    }
}

// Прост тест ендпойнт
app.MapGet("/", () => "IARA API is running! Use Swagger at root for API documentation.");
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

// Помощни методи за инициализация
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

// Метод за добавяне на семпъл данни
static async Task SeedSampleData(IARAContext context)
{
    Console.WriteLine("📊 Добавяне на семпъл данни...");
    
    // 1. Създаване на примерни рибари
    var fisher1 = new Fisher 
    { 
        FirstName = "Иван", 
        LastName = "Петров", 
        PersonalNumber = "8001011234",
        Email = "ivan.petrov@example.com",
        Phone = "+359888111222"
    };
    
    var fisher2 = new Fisher 
    { 
        FirstName = "Георги", 
        LastName = "Иванов", 
        PersonalNumber = "7505055678",
        Email = "georgi.ivanov@example.com",
        Phone = "+359888333444"
    };
    
    var fisher3 = new Fisher 
    { 
        FirstName = "Мария", 
        LastName = "Димитрова", 
        PersonalNumber = "9009109012",
        Email = "maria.dimitrova@example.com",
        Phone = "+359888555666"
    };
    
    context.Fishers.AddRange(fisher1, fisher2, fisher3);
    await context.SaveChangesAsync();
    Console.WriteLine("✅ Добавени 3 рибаря");
    
    // 2. Създаване на примерни кораби
    var ship1 = new Ship 
    { 
        Name = "Посейдон", 
        InternationalNumber = "IMO1234567",
        CallSign = "LZ1234",
        Marking = "PDN-001",
        RegistrationNumber = "BG-001",
        HomePort = "Варна",
        Length = 15.5m,
        Width = 4.2m,
        GrossTonnage = 45.8m,
        Draught = 2.1m,
        EnginePower = 250,
        EngineType = "Diesel",
        FuelType = "Diesel",
        AverageFuelConsumptionPerHour = 25,
        BuiltYear = new DateTime(2015, 1, 1),
        IsLargeShip = true,
        OwnerFisherId = fisher1.Id,
        CaptainFisherId = fisher1.Id,
        IsActive = true
    };
    
    var ship2 = new Ship 
    { 
        Name = "Нептун", 
        InternationalNumber = "IMO7654321",
        CallSign = "LZ5678",
        Marking = "NPT-002",
        RegistrationNumber = "BG-002",
        HomePort = "Бургас",
        Length = 8.5m,
        Width = 2.8m,
        GrossTonnage = 18.3m,
        Draught = 1.5m,
        EnginePower = 120,
        EngineType = "Diesel",
        FuelType = "Diesel",
        AverageFuelConsumptionPerHour = 15,
        BuiltYear = new DateTime(2018, 1, 1),
        IsLargeShip = false,
        OwnerFisherId = fisher2.Id,
        CaptainFisherId = fisher2.Id,
        IsActive = true
    };
    
    context.Ships.AddRange(ship1, ship2);
    await context.SaveChangesAsync();
    Console.WriteLine("✅ Добавени 2 кораба");
    
    // 3. Създаване на примерни разрешителни
    var license1 = new License
    {
        LicenseNumber = "LIC-2024-001",
        FisherId = fisher1.Id,
        ShipId = ship1.Id,
        IssueDate = DateTime.Now.AddMonths(-6),
        ExpiryDate = DateTime.Now.AddDays(15), // Изтича след 15 дни (Report 1 ще покаже това)
        Status = "Active",
        LicenseType = "Професионален риболов",
        IssuingAuthority = "ИАРА Варна"
    };
    
    var license2 = new License
    {
        LicenseNumber = "LIC-2024-002",
        FisherId = fisher2.Id,
        ShipId = ship2.Id,
        IssueDate = DateTime.Now.AddMonths(-12),
        ExpiryDate = DateTime.Now.AddMonths(6),
        Status = "Active",
        LicenseType = "Професионален риболов",
        IssuingAuthority = "ИАРА Бургас"
    };
    
    context.Licenses.AddRange(license1, license2);
    await context.SaveChangesAsync();
    Console.WriteLine("✅ Добавени 2 разрешителни (едното изтича след 15 дни)");
    
    // 4. Създаване на примерни билети за любители
    var amateurTicket = new AmateurTicket
    {
        FisherId = fisher3.Id,
        TicketNumber = "TICKET-2024-001",
        IssueDate = DateTime.Now.AddMonths(-3),
        ExpiryDate = DateTime.Now.AddMonths(9),
        Status = "Active",
        IssuingAuthority = "ИАРА Онлайн"
    };
    
    context.AmateurTickets.Add(amateurTicket);
    await context.SaveChangesAsync();
    Console.WriteLine("✅ Добавен билет за любителски риболов");
    
    // 5. Създаване на примерни улов за любители (за Report 2)
    var amateurCatch1 = new AmateurCatch
    {
        AmateurTicketId = amateurTicket.Id,
        CatchDate = DateTime.Now.AddMonths(-1),
        FishSpecies = "Каракуда",
        WeightKgs = 12.5m,
        Quantity = 3,
        FishingLocation = "Язовир Ивайловград",
        FishingMethod = "Въдица"
    };
    
    var amateurCatch2 = new AmateurCatch
    {
        AmateurTicketId = amateurTicket.Id,
        CatchDate = DateTime.Now.AddMonths(-2),
        FishSpecies = "Сардина",
        WeightKgs = 8.3m,
        Quantity = 25,
        FishingLocation = "Черно море - Поморие",
        FishingMethod = "Въдица"
    };
    
    context.AmateurCatches.AddRange(amateurCatch1, amateurCatch2);
    await context.SaveChangesAsync();
    Console.WriteLine("✅ Добавени 2 улова за любители (общо 20.8кг)");
    
    // 6. Създаване на примерни записи в дневник (за Reports 3 & 4)
    var logbook1 = new LogbookEntry
    {
        LicenseId = license1.Id,
        FishingDate = DateTime.Now.AddMonths(-1),
        StartTime = new TimeSpan(6, 0, 0),
        EndTime = new TimeSpan(14, 0, 0),
        FishingArea = "Черно море - север, сектор 12",
        FuelConsumptionLiters = 200,
        DistanceTraveled = 85,
        WeatherConditions = "Слънчево, вятър 3-4 Bf"
    };
    
    var logbook2 = new LogbookEntry
    {
        LicenseId = license1.Id,
        FishingDate = DateTime.Now.AddMonths(-2),
        StartTime = new TimeSpan(5, 30, 0),
        EndTime = new TimeSpan(16, 0, 0),
        FishingArea = "Черно море - юг, сектор 8",
        FuelConsumptionLiters = 250,
        DistanceTraveled = 120,
        WeatherConditions = "Облачно, вятър 2-3 Bf"
    };
    
    var logbook3 = new LogbookEntry
    {
        LicenseId = license2.Id,
        FishingDate = DateTime.Now.AddMonths(-1),
        StartTime = new TimeSpan(7, 0, 0),
        EndTime = new TimeSpan(13, 30, 0),
        FishingArea = "Черно море - изток, сектор 5",
        FuelConsumptionLiters = 90,
        DistanceTraveled = 45,
        WeatherConditions = "Слънчево, спокойно море"
    };
    
    context.LogbookEntries.AddRange(logbook1, logbook2, logbook3);
    await context.SaveChangesAsync();
    Console.WriteLine("✅ Добавени 3 записа в дневника");
    
    // 7. Създаване на примерни детайли за улов
    var catchDetail1 = new CatchDetail
    {
        LogbookEntryId = logbook1.Id,
        FishSpecies = "Скумрия",
        WeightKgs = 150.5m,
        Quantity = 120,
        FishingGear = "Мрежа",
        Notes = "Качествен улов"
    };
    
    var catchDetail2 = new CatchDetail
    {
        LogbookEntryId = logbook1.Id,
        FishSpecies = "Херинга",
        WeightKgs = 85.3m,
        Quantity = 95,
        FishingGear = "Мрежа",
        Notes = "Стандартен улов"
    };
    
    var catchDetail3 = new CatchDetail
    {
        LogbookEntryId = logbook2.Id,
        FishSpecies = "Паламуд",
        WeightKgs = 180.3m,
        Quantity = 45,
        FishingGear = "Въдица",
        Notes = "Добър улов"
    };
    
    var catchDetail4 = new CatchDetail
    {
        LogbookEntryId = logbook3.Id,
        FishSpecies = "Кефал",
        WeightKgs = 65.8m,
        Quantity = 38,
        FishingGear = "Мрежа",
        Notes = "Среден улов"
    };
    
    context.CatchDetails.AddRange(catchDetail1, catchDetail2, catchDetail3, catchDetail4);
    await context.SaveChangesAsync();
    Console.WriteLine("✅ Добавени 4 детайла за улов");
    
    // 8. Създаване на примерни инспекции
    var inspection1 = new Inspection
    {
        LicenseId = license1.Id,
        ShipId = ship1.Id,
        InspectorId = 1, // Администраторът е инспектор
        InspectionDate = DateTime.Now.AddMonths(-1),
        InspectionType = "Рутинна проверка",
        Status = "Completed",
        Violations = "Няма",
        ActionsTaken = "Всичко е наред"
    };
    
    context.Inspections.Add(inspection1);
    await context.SaveChangesAsync();
    Console.WriteLine("✅ Добавена инспекция");
    
    Console.WriteLine("🎉 Всички семпъл данни са добавени успешно!");
    Console.WriteLine("📊 Сега можете да тествате 4-те справки:");
    Console.WriteLine("   1. /api/Reports/expiring-licenses - ще покаже 1 разрешително");
    Console.WriteLine("   2. /api/Reports/amateur-ranking - ще покаже 1 любител с 20.8кг");
    Console.WriteLine("   3. /api/Reports/ship-catch-analysis/2024 - статистика за корабите");
    Console.WriteLine("   4. /api/Reports/ship-fuel-efficiency/2024 - въглероден отпечатък");
}