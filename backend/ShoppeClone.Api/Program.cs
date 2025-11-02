using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

using FluentValidation;
using FluentValidation.AspNetCore;
using Polly;
using Polly.Extensions.Http;

using ShoppeClone.Api.Infrastructure;
using ShoppeClone.Api.Application.AI.Clients;
using ShoppeClone.Api.Application.AI.Interfaces;
using ShoppeClone.Api.Application.AI.Services;
using ShoppeClone.Api.Infrastructure.Repositories;
using ShoppeClone.Api.Application.Payment;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// MVC + CORS
builder.Services.AddControllers();
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("mobile", p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});

// JWT
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

// Swagger + Bearer
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
        Description = "Dán token: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });
});

// (Giữ nguyên các service AI của bạn)
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RagChatRequestValidator>();
builder.Services.Configure<GoogleAiOptions>(builder.Configuration.GetSection("GoogleAI"));
builder.Services.AddHttpClient<GeminiClient>(c =>
{
    c.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
    c.Timeout = TimeSpan.FromSeconds(60);
}).AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError()
    .WaitAndRetryAsync(new[] { TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1) }));

builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
builder.Services.AddScoped<IKbRepository, KbRepository>();
builder.Services.AddScoped<IRagService, RagService>();

// VNPay
builder.Services.AddScoped<VNPayService>();
builder.Services.AddHttpClient();

var app = builder.Build();

// Lấy IP thật qua ngrok/proxy
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownNetworks = { },
    KnownProxies = { }
});

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("mobile");
app.UseAuthentication();
app.UseAuthorization();

// Route kiểm tra nhanh
app.MapGet("/health", () => Results.Ok("OK"));

app.MapControllers();

app.Logger.LogInformation("VNPay ReturnUrl: {ReturnUrl}", builder.Configuration["VNPay:ReturnUrl"]);
app.Run();
