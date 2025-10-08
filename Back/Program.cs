using Back.Data;
using Microsoft.EntityFrameworkCore;
using Back.Models.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Back.Services;
using Back.Policies;
using Microsoft.AspNetCore.CookiePolicy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// JWT Authentication with Cookies
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
        
        // Чтение токена из куки
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.ContainsKey("access_token"))
                {
                    context.Token = context.Request.Cookies["access_token"];
                }
                return Task.CompletedTask;
            }
        };
    })
    .AddCookie("Cookies", options =>
    {
        options.Cookie.Name = "access_token";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
            ? CookieSecurePolicy.None 
            : CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
    });

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    AuthorizationPolicies.ConfigurePolicies(options);
});

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure CORS if needed
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Адрес вашего фронтенда
              .AllowCredentials()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS must be before Authentication and Authorization
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Cookie policy
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = builder.Environment.IsDevelopment() 
        ? CookieSecurePolicy.None 
        : CookieSecurePolicy.Always
});

app.MapControllers();

// Apply migrations on startup (optional)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
    
    // Create default admin user if not exists
    await CreateDefaultAdminUser(dbContext);
}

app.Run();

async Task CreateDefaultAdminUser(AppDbContext context)
{
    if (!context.Users.Any())
    {
        var authService = app.Services.CreateScope().ServiceProvider.GetRequiredService<IAuthService>();
        
        var adminUser = new User
        {
            Login = "admin",
            Fio = "Administrator",
            Hash = authService.HashPassword("admin123"),
            RoleId = 3 // Manager
        };
        
        context.Users.Add(adminUser);
        await context.SaveChangesAsync();
        
        Console.WriteLine("Default admin user created: admin / admin123");
    }
}