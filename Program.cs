// using System.Text;
// using EventHub.Data;
// using EventHub.Models;
// using EventHub.Services;
// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.AspNetCore.Builder;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Microsoft.IdentityModel.Tokens;
// using Microsoft.OpenApi.Models;
// using Telegram.Bot;
// using System.Security.Claims;

// // Для Newtonsoft.Json
// using Newtonsoft.Json.Converters;
// using Newtonsoft.Json.Serialization;

// var builder = WebApplication.CreateBuilder(args);
// var config = builder.Configuration;

// // 1. Настройка JWT
// builder.Services.Configure<JwtOptions>(config.GetSection("Jwt"));

// // 2. Аутентификация JWT Bearer
// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//     options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
// })
// .AddJwtBearer(options =>
// {
//     options.MapInboundClaims = false;
//     options.TokenValidationParameters.NameClaimType = "id";
//     options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;

//     var jwtOpts = config.GetSection("Jwt").Get<JwtOptions>()!;
//     options.RequireHttpsMetadata = false;
//     options.SaveToken = true;
//     options.TokenValidationParameters = new TokenValidationParameters
//     {
//         ValidateIssuer           = true,
//         ValidateAudience         = true,
//         ValidateLifetime         = true,
//         ValidateIssuerSigningKey = true,
//         ValidIssuer              = jwtOpts.Issuer,
//         ValidAudience            = jwtOpts.Audience,
//         IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOpts.Key))
//     };
// });

// // 3. EF Core + Identity
// builder.Services.AddDbContext<EventHubDbContext>(opts =>
//     opts.UseNpgsql(config.GetConnectionString("DefaultConnection")));

// builder.Services.AddIdentityCore<User>(options =>
// {
//     options.Password.RequireDigit           = true;
//     options.Password.RequireUppercase       = false;
//     options.Password.RequireLowercase       = false;
//     options.Password.RequireNonAlphanumeric = false;
//     options.Password.RequiredLength         = 6;
// })
// .AddRoles<Role>()
// .AddEntityFrameworkStores<EventHubDbContext>()
// .AddDefaultTokenProviders();

// // 4. Telegram Bot client
// builder.Services.AddSingleton<ITelegramBotClient>(_ =>
//     new TelegramBotClient(config["Telegram:BotToken"]!));

// // 5. Приложенческие сервисы
// builder.Services.AddScoped<JwtService>();
// builder.Services.AddScoped<IUserService, UserService>();
// builder.Services.AddHostedService<NotificationHostedService>();

// // 6. CORS
// builder.Services.AddCors(o => o.AddPolicy("AllowAll", p =>
// {
//     p.AllowAnyOrigin()
//      .AllowAnyMethod()
//      .AllowAnyHeader();
// }));

// // 7. MVC + Newtonsoft.Json (обязательно, чтобы конвертер Unix-DateTime заработал!)
// builder.Services.AddControllers()
//     .AddNewtonsoftJson(opts =>
//     {
//         // enum → camelCase string
//         opts.SerializerSettings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
//         // PascalCase для свойств
//         opts.SerializerSettings.ContractResolver = new DefaultContractResolver();
//     });

// // 8. Swagger / OpenAPI
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen(c =>
// {
//     c.SwaggerDoc("v1", new OpenApiInfo { Title = "EventHub API", Version = "v1" });
//     c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//     {
//         Description = "JWT Authorization header using the Bearer scheme",
//         Name        = "Authorization",
//         In          = ParameterLocation.Header,
//         Type        = SecuritySchemeType.Http,
//         Scheme      = "bearer"
//     });
//     c.AddSecurityRequirement(new OpenApiSecurityRequirement
//     {
//         {
//             new OpenApiSecurityScheme
//             {
//                 Reference = new OpenApiReference 
//                 { 
//                     Type = ReferenceType.SecurityScheme, 
//                     Id   = "Bearer" 
//                 }
//             },
//             Array.Empty<string>()
//         }
//     });
//     c.CustomSchemaIds(type => type.FullName);
// });

// var app = builder.Build();

// // 9. Пайплайн
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }
// app.UseCors("AllowAll");
// app.UseAuthentication();
// app.UseAuthorization();

// // Гарантия JSON-ответа
// app.Use(async (ctx, next) =>
// {
//     await next();
//     if (!ctx.Response.Headers.ContainsKey("Content-Type"))
//         ctx.Response.Headers["Content-Type"] = "application/json; charset=utf-8";
// });

// app.MapControllers();
// app.UseHttpsRedirection();
// app.Run();


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

var builder = WebApplication.CreateBuilder(args);
var config  = builder.Configuration;

// 1) Конфигурация JwtOptions из appsettings.json
builder.Services.Configure<JwtOptions>(config.GetSection("Jwt"));

// 2) Добавляем аутентификацию по JWT
builder.Services
    .AddAuthentication(options =>
    {
        // ставим схему по-умолчанию
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(opts =>
    {
        var jwt = config.GetSection("Jwt").Get<JwtOptions>()!;
        opts.RequireHttpsMetadata   = false;
        opts.SaveToken              = true;
        // Отключаем автоматическое маппинг inbound-клеймов Telegram.Bot
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
        // остальные настройки пароля как у Вас
    })
    .AddRoles<Role>()
    .AddEntityFrameworkStores<EventHubDbContext>()
    .AddDefaultTokenProviders();

// 4) Telegram client
builder.Services.AddSingleton<ITelegramBotClient>(_ =>
    new TelegramBotClient(config["Telegram:BotToken"]!));

// 5) Ваши сервисы
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHostedService<NotificationHostedService>();

// 6) CORS
builder.Services.AddCors(o => o.AddPolicy("AllowAll", p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

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

app.UseCors("AllowAll");

// Обязательно между UseRouting и MapControllers
app.UseAuthentication();
app.UseAuthorization();

// Гарантируем JSON-заголовок
app.Use(async (ctx, next) =>
{
    await next();
    if (!ctx.Response.Headers.ContainsKey("Content-Type"))
        ctx.Response.Headers["Content-Type"] = "application/json; charset=utf-8";
});

app.MapControllers();
app.UseHttpsRedirection();

app.Run();
