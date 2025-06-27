using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EventHub.Data;
using EventHub.Models;
using EventHub.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Telegram.Bot;

// Againts 400 error in JSON-body from Telegram
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// 1. JWT
builder.Services.Configure<JwtOptions>(config.GetSection("Jwt"));

// 2. Authentication & JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtOpts = config.GetSection("Jwt").Get<JwtOptions>()!;
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtOpts.Issuer,
        ValidAudience            = jwtOpts.Audience,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOpts.Key))
    };
});

// 3. EF Core + Identity
builder.Services.AddDbContext<EventHubDbContext>(opts =>
    opts.UseNpgsql(config.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityCore<User>(options =>
{
    options.Password.RequireDigit           = true;
    options.Password.RequireUppercase       = false;
    options.Password.RequireLowercase       = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength         = 6;
})
.AddRoles<Role>()
.AddEntityFrameworkStores<EventHubDbContext>()
.AddDefaultTokenProviders();

// 4. Telegram Bot client
builder.Services.AddSingleton<ITelegramBotClient>(_ =>
    new Telegram.Bot.TelegramBotClient(config["Telegram:BotToken"]!));

// 5. Application services
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHostedService<NotificationHostedService>();

// 6. CORS
builder.Services.AddCors(o => o.AddPolicy("AllowAll", p =>
{
    p.AllowAnyOrigin()
     .AllowAnyMethod()
     .AllowAnyHeader();
}));

// 7. Controllers + System.Text.Json settings
builder.Services.AddControllers()
    // **Используем Newtonsoft.Json вместо System.Text.Json:**
    .AddNewtonsoftJson(options =>
    {
        // Enums => в camelCase строку (как было настроено ранее)
        options.SerializerSettings.Converters.Add(
            new StringEnumConverter(new CamelCaseNamingStrategy()));
        // Имена свойств – в PascalCase (отключаем кемелизацию)
        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
    });

// 8. Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EventHub API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name        = "Authorization",
        In          = ParameterLocation.Header,
        Type        = SecuritySchemeType.Http,
        Scheme      = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    c.CustomSchemaIds(type => type.FullName);
});

var app = builder.Build();
// 9. Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Гарантируем, что без Content-Type всё равно уйдёт JSON
app.Use(async (ctx, next) =>
{
    await next();
    if (!ctx.Response.Headers.ContainsKey("Content-Type"))
        ctx.Response.Headers["Content-Type"] = "application/json; charset=utf-8";
});

app.MapControllers();
app.UseHttpsRedirection();
app.Run();
