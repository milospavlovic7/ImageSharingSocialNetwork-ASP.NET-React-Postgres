using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using AspNetCorePostgreSQLDockerApp.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using NSwag;
using NSwag.Generation.Processors.Security;


var builder = WebApplication.CreateBuilder(args); //Kreiramo instancu WebApplicationBuilder klase


// Konfiguriši logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole().AddDebug();

// Konfiguriši logging pomoću LoggerFactory
ILogger<Program> logger = LoggerFactory.Create(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
}).CreateLogger<Program>();

logger.LogInformation("Pokretanje aplikacije...");

// Configuration
var configuration = builder.Configuration;

// Services Registration
try
{
    builder.Services.AddDbContext<DatabaseDbContext>(options =>
        options.UseNpgsql(configuration["Data:DbContext:DatabaseConnectionString"]));
    logger.LogInformation("DbContext konfigurisan uspešno.");
}
catch (Exception ex)
{
    logger.LogError(ex, "Greška prilikom konfiguracije DbContext-a.");
    throw;
}

// XmlRepository
try
{
    builder.Services.AddScoped<IXmlRepository, XmlRepository>();
    logger.LogInformation("XmlRepository registrovan uspešno.");
}
catch (Exception ex)
{
    logger.LogError(ex, "Greška prilikom registracije XmlRepository.");
    throw;
}

// Data Protection
try
{
#if !DEBUG
    var certPath = "/var/www/aspnetcoreapp/certs/mydevelopmentcert.pfx";
    if (!File.Exists(certPath))
    {
        logger.LogError("Sertifikat nije pronađen na putanji: {Path}", certPath);
        throw new FileNotFoundException("Certificate not found.", certPath);
    }

    var certificate = new X509Certificate2(certPath, "password");

    builder.Services.AddDataProtection()
        .SetApplicationName("MyAPP")
        .PersistKeysToFileSystem(new DirectoryInfo("/var/www/aspnetcoreapp/keys"))
        .ProtectKeysWithCertificate(certificate);
#endif

    logger.LogInformation("DataProtection konfigurisan uspešno.");
}
catch (Exception ex)
{
    logger.LogError(ex, "Greška prilikom konfiguracije DataProtection.");
    throw;
}

// Repositories
try
{
    builder.Services.AddScoped<IUsersRepository, UsersRepository>();
    builder.Services.AddScoped<IImagesRepository, ImagesRepository>();
    builder.Services.AddScoped<ICommentsRepository, CommentsRepository>();
    builder.Services.AddScoped<ILikesRepository, LikesRepository>();

    logger.LogInformation("Repositoryji registrovani uspešno.");
}
catch (Exception ex)
{
    logger.LogError(ex, "Greška prilikom registracije Repository.");
    throw;
}

// Database Seeder
try
{
    builder.Services.AddTransient<DatabaseDbSeeder>();
    logger.LogInformation("DatabaseDbSeeder registrovan uspešno.");
}
catch (Exception ex)
{
    logger.LogError(ex, "Greška prilikom registracije DatabaseDbSeeder.");
    throw;
}

// Swagger Configuration with Detailed Logging and Updated Swashbuckle Features
builder.Services.AddOpenApiDocument(options =>
{
    options.DocumentName = "v1";
    options.PostProcess = document =>
    {
        document.Info.Version = "2.0";
        document.Info.Title = "Application API";
        document.Info.Description = "Application Documentation";
        document.Info.Contact = new NSwag.OpenApiContact
        {
            Name = "Author",
            Email = "author@example.com",
            Url = "https://example.com"
        };
        document.Info.License = new NSwag.OpenApiLicense
        {
            Name = "MIT",
            Url = "https://opensource.org/licenses/MIT"
        };
    };

    // Omogućavanje Bearer autentifikacije
    options.AddSecurity("Bearer", new OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
        Name = "Authorization",
        Description = "Unesite JWT token u polje 'Authorization'."
    });

    options.OperationProcessors.Add(new OperationSecurityScopeProcessor("Bearer"));
});


// CORS Configuration
builder.Services.AddCors(o => o.AddPolicy("AllowAllPolicy", options =>
{
    options.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
}));

// Add controllers to the services collection
builder.Services.AddControllers();

// JWT Authentication
try
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "http://localhost:5000",
            ValidAudience = "http://localhost:3000",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretKeyForJWTEncryption123456!"))
        };
    });

    logger.LogInformation("JWT Authentication konfigurisan uspešno.");
}
catch (Exception ex)
{
    logger.LogError(ex, "Greška prilikom konfiguracije JWT Authentication.");
    throw;
}

// Dodaj autorizaciju u servisima
builder.Services.AddAuthorization();

// Static Files for SPA
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "build";
});

var app = builder.Build();

// Development Environment Configuration
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    logger.LogInformation("Development okruženje aktivno.");
}
else
{
    app.UseExceptionHandler("/Home/Error");
    logger.LogInformation("Production okruženje aktivno.");
}

// Middleware Configuration
app.UseCors("AllowAllPolicy");

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images")),
    RequestPath = "/images",
    OnPrepareResponse = ctx =>
    {
        // Dodajte log za svaki zahtev za sliku
        Console.WriteLine($"Serving file: {ctx.File.Name}");
    }
});


app.UseSpaStaticFiles();


app.UseOpenApi(); // Generiše OpenAPI dokument (swagger.json)
app.UseSwaggerUi(); // Pokreće Swagger UI v3


app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
//app.MapFallbackToController("Index", "Database");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbSeeder = services.GetRequiredService<DatabaseDbSeeder>();
        logger.LogInformation("Pokretanje seeder-a baze podataka.");
        await dbSeeder.SeedAsync(services);
        logger.LogInformation("Seeder baze podataka uspešno završen.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Greška prilikom izvršavanja seeder-a baze podataka.");
    }
}

logger.LogInformation("Aplikacija je spremna za rad.");
app.Run();
