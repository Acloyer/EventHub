<<<<<<< HEAD
﻿// using System.Text;
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
=======
﻿using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
>>>>>>> 573f3e0705c1e3252b4cddd7cfc9446f4bee2932
using EventHub.Data;
using EventHub.Models;
using EventHub.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
<<<<<<< HEAD
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
=======
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
>>>>>>> 573f3e0705c1e3252b4cddd7cfc9446f4bee2932
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHostedService<NotificationHostedService>();

<<<<<<< HEAD
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
=======
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
>>>>>>> 573f3e0705c1e3252b4cddd7cfc9446f4bee2932
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EventHub API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
<<<<<<< HEAD
        Name        = "Authorization",
        In          = ParameterLocation.Header,
        Type        = SecuritySchemeType.Http,
        Scheme      = "bearer",
        Description = "JWT"
=======
        Description = "JWT Authorization header using the Bearer scheme",
        Name        = "Authorization",
        In          = ParameterLocation.Header,
        Type        = SecuritySchemeType.Http,
        Scheme      = "bearer"
>>>>>>> 573f3e0705c1e3252b4cddd7cfc9446f4bee2932
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
<<<<<<< HEAD
    c.CustomSchemaIds(t => t.FullName);
});

var app = builder.Build();

// 9) Middleware pipeline
=======
    c.CustomSchemaIds(type => type.FullName);
});

var app = builder.Build();
// 9. Middleware pipeline
>>>>>>> 573f3e0705c1e3252b4cddd7cfc9446f4bee2932
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
<<<<<<< HEAD

// Обязательно между UseRouting и MapControllers
app.UseAuthentication();
app.UseAuthorization();

// Гарантируем JSON-заголовок
=======
app.UseAuthentication();
app.UseAuthorization();

// Гарантируем, что без Content-Type всё равно уйдёт JSON
>>>>>>> 573f3e0705c1e3252b4cddd7cfc9446f4bee2932
app.Use(async (ctx, next) =>
{
    await next();
    if (!ctx.Response.Headers.ContainsKey("Content-Type"))
        ctx.Response.Headers["Content-Type"] = "application/json; charset=utf-8";
});
<<<<<<< HEAD

app.MapControllers();
app.UseHttpsRedirection();
=======
>>>>>>> 573f3e0705c1e3252b4cddd7cfc9446f4bee2932

app.MapControllers();
app.UseHttpsRedirection();
app.Run();
