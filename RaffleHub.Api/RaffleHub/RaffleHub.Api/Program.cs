using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaffleHub.Api.Data;
using RaffleHub.Api.Repositories;
using RaffleHub.Api.Repositories.Interfaces;
using RaffleHub.Api.Services;
using RaffleHub.Api.Services.Interface;
using RaffleHub.Api.Utils;
using RaffleHub.Api.Utils.ExceptionHandler;
using RaffleHub.Api.Utils.Job;
using RaffleHub.Api.Utils.Mappings;
using RaffleHub.Api.Services.MercadoPago;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;
using Polly;
using Polly.Extensions.Http;
using System.Text;
using System.Threading.RateLimiting;
using RaffleHub.Api.Entities;
using RaffleHub.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Seq(builder.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value != null && x.Value.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        var response = ApiResponse<object>.Fail("Alguns erros de validação ocorreram", errors);

        return new BadRequestObjectResult(response);
    };
});

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IRaffleRepository, RaffleRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IParticipantRepository, ParticipantRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IGalleryRepository, GalleryRepository>();
builder.Services.AddScoped<ICategoriesGalleryRepository, CategoriesGalleryRepository>();

builder.Services.AddSingleton<ISupabaseService, SupabaseService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<ParticipantService>();
builder.Services.AddScoped<RaffleService>();
builder.Services.AddScoped<GalleryService>();
builder.Services.AddScoped<CategoriesGalleryService>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddHttpClient<IMercadoPagoService, MercadoPagoService>()
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

builder.Services.AddHttpClient<IN8nService, N8nService>()
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddHealthChecks();
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddHealthChecks().AddNpgSql(connectionString);
}


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy => policy.WithOrigins(
            "https://rifa-frontend-iota.vercel.app", 
            "http://localhost:4200", 
            "https://yelping-lyssa-iff-road-715cb09c.koyeb.app",
            "https://rafflehub-git-development-suelenmedinap.vercel.app",
            "http://localhost:8080"
        )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.AddFixedWindowLimiter("PixLimit", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 3;
        opt.QueueLimit = 0;
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is missing in configuration.")))
    };
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(MappingProfile).Assembly);

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddHangfire(config =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString) || builder.Environment.EnvironmentName == "Testing")
    {
        config.UseInMemoryStorage();
    }
    else
    {
        config.UsePostgreSqlStorage(options =>
            options.UseNpgsqlConnection(connectionString));
    }
});

builder.Services.AddHangfireServer();
builder.Services.AddScoped<RaffleJob>();
builder.Services.AddScoped<BookingJob>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme.",
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

app.UseHangfireDashboard("/hangfire");
RecurringJob.AddOrUpdate<RaffleJob>(
    "expiration-raffles",
    job => job.ExpireRaffles(),
    "0 3 * * *"
);

RecurringJob.AddOrUpdate<BookingJob>(
    "expiration-bookings",
    job => job.ExpirePixAsync(),
    Cron.Minutely
);

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (context.Database.IsRelational())
    {
        if (!app.Environment.IsProduction() && !app.Environment.IsEnvironment("Testing"))
        {
            await context.Database.MigrateAsync();
        }
        else if (app.Environment.IsProduction())
        {
            Log.Information("Ambiente de Produção detectado. As migrações automáticas foram ignoradas. Certifique-se de rodá-las manualmente ou via pipeline.");
        }
    }

    var supabaseService = scope.ServiceProvider.GetRequiredService<ISupabaseService>();
    if (supabaseService is SupabaseService concreteService)
    {
        await concreteService.InitializeAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.MapHealthChecks("/health");
app.UseRateLimiter();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<PaymentNotificationHub>("/payment-hub");
app.Run();

public partial class Program { }