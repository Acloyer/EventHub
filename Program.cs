using EventHub.Data;
using EventHub.Models;
using EventHub.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ─── 1) Добавляем все сервисы ──────────────────────────────────────────────

// 1.1 EF + Postgres
builder.Services.AddDbContext<EventHubDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 1.2 Swagger с поддержкой Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "EventHub API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите: Bearer {токен}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        }] = Array.Empty<string>()
    });
});

var jwtSection = builder.Configuration.GetSection("Jwt");
var key = jwtSection["Key"] ?? throw new Exception("JWT Key не задан!");
var issuer = jwtSection["Issuer"] ?? throw new Exception("JWT Issuer не задан!");
var audience = jwtSection["Audience"] ?? throw new Exception("JWT Audience не задан!");
var expireMin = int.Parse(jwtSection["ExpireMinutes"] ?? "60");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };
    });

// 1.5 Authorization и Controllers
builder.Services.AddAuthorization();
builder.Services.AddControllers();

// 1.6 Telegram Bot API
builder.Services.AddHttpClient();
builder.Services.AddHostedService<NotificationHostedService>();


// ─── 2) Строим приложение и задаём Pipeline ──────────────────────────────


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EventHubDbContext>();
    if (!db.Roles.Any())
    {
        db.Roles.AddRange(
            new Role { Name = "Admin" },
            new Role { Name = "Organizer" },
            new Role { Name = "Moderator" },
            new Role { Name = "User" }
        );
        db.SaveChanges();
    }


    // 1) Роли
    if (!db.Roles.Any())
    {
        db.Roles.AddRange(
            new Role { Name = "Admin" },
            new Role { Name = "Organizer" },
            new Role { Name = "Moderator" },
            new Role { Name = "User" }
        );
        db.SaveChanges();
    }

    // 2) Админ-пользователь string/string
    const string adminEmail = "string123";
    const string adminPassword = "string123";

    // проверяем, есть ли уже такой юзер
    if (!db.Users.Any(u => u.Email == adminEmail))
    {
        // хешируем пароль
        var hash = BCrypt.Net.BCrypt.HashPassword(adminPassword);

        // создаём запись в Users
        var admin = new User
        {
            Email = adminEmail,
            PasswordHash = hash
        };
        db.Users.Add(admin);
        db.SaveChanges();

        // привязываем роль Admin
        var adminRoleId = db.Roles.Single(r => r.Name == "Admin").Id;
        db.UserRoles.Add(new UserRole
        {
            UserId = admin.Id,
            RoleId = adminRoleId
        });
        db.SaveChanges();
    }
}

app.MapControllers();
app.Run();
