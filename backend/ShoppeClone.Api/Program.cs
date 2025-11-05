using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ShoppeClone.Api.Infrastructure;
using System.Text;
using ShoppeClone.Api.Application.AI.Clients;
using ShoppeClone.Api.Application.AI.Interfaces;
using ShoppeClone.Api.Application.AI.Services;
using ShoppeClone.Api.Infrastructure.Repositories;
using ShoppeClone.Api.Application.Payment;
using Microsoft.AspNetCore.Mvc;

using FluentValidation;
using FluentValidation.AspNetCore;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// DbContext
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));


// MVC + CORS
builder.Services.AddControllers();
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("mobile", p => p
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin());
});

// Auth (JWT)
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero // tránh lệch giờ khi kiểm tra hết hạn
    };

    // (Tùy chọn) Cho phép gửi token trần (không cần "Bearer ")
    // options.Events = new JwtBearerEvents
    // {
    //     OnMessageReceived = ctx =>
    //     {
    //         var h = ctx.Request.Headers["Authorization"].FirstOrDefault();
    //         if (!string.IsNullOrEmpty(h) && !h.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
    //             ctx.Token = h;
    //         return Task.CompletedTask;
    //     }
    // };
});

// Swagger + Bearer "Authorize"
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ShoppeClone API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Dán token theo dạng: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RagChatRequestValidator>();

// Options
builder.Services.Configure<GoogleAiOptions>(builder.Configuration.GetSection("GoogleAI"));

// Typed HttpClient (base URL + retry)
builder.Services.AddHttpClient<GeminiClient>(c =>
{
    c.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
    c.Timeout = TimeSpan.FromSeconds(60);
})
.AddPolicyHandler(Polly.Extensions.Http.HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(new[]
    {
        TimeSpan.FromMilliseconds(200),
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromSeconds(1)
    }));

builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
builder.Services.AddScoped<IKbRepository, KbRepository>();
builder.Services.AddScoped<IRagService, RagService>();

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("mobile", p => p
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithOrigins(
            "http://localhost:3000", // React web
            "http://localhost:3001",
            "exp://*", // Expo
            "http://*", // Mobile app
            "https://*" // Mobile app HTTPS
        ));
});

// Cấu hình VNPay
builder.Services.Configure<VNPayConfiguration>(builder.Configuration.GetSection("VNPay"));

// Đăng ký services VNPay - SỬA PHẦN NÀY
if (builder.Environment.IsDevelopment())
{
    // Development: Dùng Mock service để test
    builder.Services.AddScoped<IVNPayService, MockVNPayService>();
    Console.WriteLine("🔧 Using Mock VNPay Service for Development");
}
else
{
    // Production: Dùng service thật
    builder.Services.AddScoped<IVNPayService, VNPayService>();
    Console.WriteLine("🚀 Using Real VNPay Service for Production");
}

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Pipeline
app.UseCors("mobile");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();