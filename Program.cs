using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Claims;
using EventHub.Data;
using EventHub.Models;
using EventHub.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Telegram.Bot;
using DotNetEnv;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// 1) JwtOptions configuration from appsettings.json
builder.Services.Configure<JwtOptions>(config.GetSection("Jwt"));

// 2) Add JWT authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(opts =>
    {
        var jwt = config.GetSection("Jwt").Get<JwtOptions>()!;
        opts.RequireHttpsMetadata   = false;
        opts.SaveToken              = true;
        opts.MapInboundClaims       = false;
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwt.Issuer,
            ValidAudience            = jwt.Audience,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
        };
    });

// 3) EF Core + Identity
builder.Services.AddDbContext<EventHubDbContext>(o =>
    o.UseNpgsql(config.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentityCore<User>(opts =>
    {
        opts.Password.RequiredLength = 6;
        opts.Password.RequireDigit   = true;
        // other password settings
    })
    .AddRoles<Role>()
    .AddEntityFrameworkStores<EventHubDbContext>()
    .AddDefaultTokenProviders();

// 4) Telegram client
builder.Services.AddSingleton<ITelegramBotClient>(_ =>
    new TelegramBotClient(Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN")!));

// 5) Your services
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<NotificationLocalizationService>();
builder.Services.AddHostedService<NotificationHostedService>();

// 6) CORS: allow only frontend and cookies
builder.Services.AddCors(o => o.AddPolicy("AllowFrontend", p =>
    p
      .WithOrigins("http://localhost:3000") // React address
      .AllowCredentials()                    // allow cookies/credentials
      .AllowAnyMethod()
      .AllowAnyHeader()
));

// 7) Controllers + System.Text.Json (PascalCase + camelCase-enum)
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = null;
        opts.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

// 8) Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EventHub API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name        = "Authorization",
        In          = ParameterLocation.Header,
        Type        = SecuritySchemeType.Http,
        Scheme      = "bearer",
        Description = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            new string[0]
        }
    });
    c.CustomSchemaIds(t => t.FullName);
});

var app = builder.Build();

// 9) Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable the CORS policy we just created
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Ensure JSON header
app.Use(async (ctx, next) =>
{
    await next();
    if (!ctx.Response.Headers.ContainsKey("Content-Type"))
        ctx.Response.Headers["Content-Type"] = "application/json; charset=utf-8";
});

app.MapControllers();

app.UseHttpsRedirection();

// Initialize Owner if not exists
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        // Ensure Owner role exists
        if (!await roleManager.RoleExistsAsync("Owner"))
        {
            await roleManager.CreateAsync(new Role { Name = "Owner" });
            logger.LogInformation("Owner role created");
        }

        // Check if Owner user exists
        var owner = await userManager.FindByEmailAsync("owner@eventhub.com");
        if (owner == null)
        {
            // Create Owner user
            owner = new User
            {
                UserName = "owner@eventhub.com",
                Email = "owner@eventhub.com",
                Name = "System Owner",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(owner, "Password123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(owner, "Owner");
                logger.LogInformation("Owner user created successfully");
            }
            else
            {
                logger.LogError($"Failed to create Owner user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            // Ensure Owner has Owner role
            var roles = await userManager.GetRolesAsync(owner);
            if (!roles.Contains("Owner"))
            {
                await userManager.AddToRoleAsync(owner, "Owner");
                logger.LogInformation("Owner role assigned to existing user");
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during Owner initialization");
    }
}

app.Run();
