using FoodOrderingSystem.API.Data;
using FoodOrderingSystem.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// CONTROLLERS
// ==========================================
builder.Services.AddControllers();

// ==========================================
// GLOBAL EXCEPTION HANDLING / RFC 7807
// ==========================================
builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ==========================================
// CORS - ALLOW WEB PROJECT
// ==========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
    {
        policy
            .WithOrigins("https://localhost:7029")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ==========================================
// SWAGGER + JWT AUTHORIZATION
// ==========================================
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Въведете вашия JWT token."
        });

    options.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference =
                        new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type =
                                Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});

// ==========================================
// PASSWORD SERVICE
// ==========================================
builder.Services.AddScoped<PasswordService>();

// ==========================================
// DATABASE
// ==========================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// ==========================================
// JWT AUTHENTICATION
// ==========================================
builder.Services.AddAuthentication(
    JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:Key"]!
                        )
                    )
            };
    });

// ==========================================
// AUTHORIZATION
// ==========================================
builder.Services.AddAuthorization();

// ==========================================
// BUILD APPLICATION
// ==========================================
var app = builder.Build();

// ==========================================
// GLOBAL EXCEPTION HANDLER
// ==========================================
app.UseExceptionHandler();

// ==========================================
// SWAGGER
// ==========================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ==========================================
// HTTPS
// ==========================================
app.UseHttpsRedirection();

// ==========================================
// CORS
// ==========================================
app.UseCors("WebClient");

// ==========================================
// AUTHENTICATION
// ==========================================
app.UseAuthentication();

// ==========================================
// AUTHORIZATION
// ==========================================
app.UseAuthorization();

// ==========================================
// CONTROLLERS
// ==========================================
app.MapControllers();

// ==========================================
// RUN
// ==========================================
app.Run();