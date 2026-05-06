using Microsoft.EntityFrameworkCore;
using SaludTotalAPI.Data;
using SaludTotalAPI.Repository;
using SaludTotalAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SaludTotalAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using SaludTotalAPI.Constants;
using Microsoft.AspNetCore.RateLimiting;
using SaludTotalAPI.Middlewares;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(dbConnectionString))
{
    throw new Exception("Conexión a la base de datos no configurada");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(dbConnectionString));

// Add services to the container.
builder.Services.AddScoped<ISpecialtyRepository, SpecialtyRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();

builder.Services.AddControllers(options =>
{
    options.CacheProfiles.Add(CacheProfiles.Default60, CacheProfiles.Profile60);
})
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });

var apiVersioningBuilder = builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;

    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

apiVersioningBuilder.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SaludTotal API",
        Version = "v1",
        Description = "API REST para la gestión de una clínica. Permite administrar pacientes, doctores, citas médicas y expedientes clínicos.",
        Contact = new OpenApiContact
        {
            Name = "Miguel Sierra",
            Email = "miguel@hotmail.com"
        }
    });

    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "SaludTotal API #2",
        Version = "v2",
        Description = "VERSION 2 API REST para la gestión de una clínica. Permite administrar pacientes, doctores, citas médicas y expedientes clínicos, se agrego endpoints para crear y asignar roles a los usuarios.",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nuestra API utiliza la Autenticación JWT usando el esquema Bearer. \n\n" +
                        "Ingresa la palabra a continuación del token generado en login.\n\n" +
                        "Ejemplo: \"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
    {
        var key = builder.Configuration.GetValue<string>("Jwt:Key");
        var issuer = builder.Configuration.GetValue<string>("Jwt:Issuer");
        var audience = builder.Configuration.GetValue<string>("Jwt:Audience");

        if (string.IsNullOrEmpty(key))
        {
            throw new Exception("Firma JWT no configurada");
        }

        options.RequireHttpsMetadata = true;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero
        };
    });


builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy(PolicyNames.AllowFrontend, CorsPolicyBuilder =>
    {
        CorsPolicyBuilder
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(60);
        opt.PermitLimit = 5;
        opt.QueueLimit = 0;
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        await context.HttpContext.Response.WriteAsync(
            "Demasiadas solicitudes en un corto periodo de tiempo. Intenta nuevamente en unos segundos.",
            cancellationToken: token);
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    app.UseSwaggerUI(options =>
    {

        options.RoutePrefix = string.Empty;

        options.DisplayRequestDuration();

        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"SaludTotal API {description.GroupName}"
            );
        }
    });
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseResponseCaching();

app.UseCors(PolicyNames.AllowFrontend);

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var db = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    await DataSeeder.SeedDataAsync(db, userManager, roleManager);
}

app.Run();
