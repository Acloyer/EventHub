using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Telegram.Bot;
using Microsoft.OpenApi.Models;
using EventHub.Data;
using EventHub.Services;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure Telegram Bot
var botToken = builder.Configuration["TelegramBot:Token"];
builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(
    botToken ?? throw new InvalidOperationException("Telegram bot token not configured")
));

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT authentication
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.FullName);
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Authorization: Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

// Add DbContext for application data
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .EnableSensitiveDataLogging() // Debug detailed SQL logs
);

// Optional separate DbContext for controllers
builder.Services.AddDbContext<EventHubDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Configure JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(
    jwtSection["Secret"] ?? throw new InvalidOperationException("JWT secret not configured")
);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("Auth failed: " + context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated: " + context.SecurityToken);
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine("Challenge: " + context.Error);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Application services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHostedService<NotificationHostedService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EventHub API V1");
        c.DefaultModelsExpandDepth(-1);
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    // 1) Seed roles
    var requiredRoles = new[] { "Admin", "User", "Organizer", "SeniorAdmin", "Owner" };
    foreach (var roleName in requiredRoles)
    {
        if (!context.Roles.Any(r => r.Name == roleName))
        {
            context.Roles.Add(new EventHub.Models.Role
            {
                Name = roleName,
                UserRoles = new List<EventHub.Models.UserRole>()
            });
        }
    }
    context.SaveChanges();

    // 2) Seed test users and assign roles
    var testUsers = new[]
    {
        new { Email = "123@mail.ru", Password = "123", RoleName = "Admin" },
        new { Email = "12@mail.ru", Password = "12", RoleName = "Organizer" },
        new { Email = "2@gmail.com", Password = "12", RoleName = "SeniorAdmin" },
        new { Email = "3@gmail.com", Password = "1", RoleName = "Owner" }
    };

    foreach (var tu in testUsers)
    {
        var user = context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefault(u => u.Email == tu.Email);

        if (user == null)
        {
            using var hmac = new HMACSHA512();
            user = new EventHub.Models.User
            {
                Email = tu.Email,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(tu.Password)),
                PasswordSalt = hmac.Key
            };
            context.Users.Add(user);
            context.SaveChanges();
        }

        var role = context.Roles.Single(r => r.Name == tu.RoleName);
        var hasRole = context.UserRoles
            .Any(ur => ur.UserId == user.Id && ur.RoleId == role.Id);
        if (!hasRole)
        {
            context.UserRoles.Add(new EventHub.Models.UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            });
            context.SaveChanges();
        }
    }

    // 3) Optionally remove default admin
    var defaultAdmin = context.Users.SingleOrDefault(u => u.Email == "admin@eventhub.com");
    if (defaultAdmin != null)
    {
        var adminRoles = context.UserRoles.Where(ur => ur.UserId == defaultAdmin.Id);
        context.UserRoles.RemoveRange(adminRoles);
        context.Users.Remove(defaultAdmin);
        context.SaveChanges();
    }
}

app.Run();
