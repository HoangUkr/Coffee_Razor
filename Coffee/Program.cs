using Application;
using Application.Interfaces;
using Application.Services;
using Application.Repositories;
using Application.DTOs.Settings;

using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Settings;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using WebUI.Hubs;
using WebUI.Services;

using System.Text;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
const string authTokenCookieName = "AuthToken";
var redisEnabled = builder.Configuration.GetValue<bool>("Redis:Enabled");
var redisConfiguration = builder.Configuration["Redis:Configuration"];
var redisInstanceName = builder.Configuration["Redis:InstanceName"] ?? "CoffeeApp:";

// Add MVC and API support
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddSignalR();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin");
    options.Conventions.AllowAnonymousToPage("/Admin/Login");
    options.Conventions.AllowAnonymousToPage("/Admin/Index");
});

// Add distributed cache / session support
if (redisEnabled && !string.IsNullOrWhiteSpace(redisConfiguration))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConfiguration;
        options.InstanceName = redisInstanceName;
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add DbContext with SQL Server provider
builder.Services.AddDbContext<CoffeeDbContext>(options => options.UseSqlServer(connectionString, b => b.MigrationsAssembly("Infrastructure")));

// Add auto mapper
builder.Services.AddAutoMapper(config => 
{
    config.AddProfile<UserProfile>();
    config.AddProfile<OrderMappingProfile>();
    config.AddProfile<ItemMappingProfile>();
    config.AddProfile<CategoryMappingProfile>();
    config.AddProfile<ItemImageMappingProfile>();
    config.AddProfile<Application.MappingProfiles.ReservationMappingProfile>();
});

// Register repository
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IItemImageRepository, ItemImageRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
builder.Services.AddScoped<IWorkingScheduleRepository, WorkingScheduleRepository>();
builder.Services.AddScoped<IHolidayRepository, HolidayRepository>();

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IItemImageService, ItemImageService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddSingleton<IStorageService, AzureBlobStorageService>();
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISystemSettingService, SystemSettingService>();
builder.Services.AddScoped<IWorkingScheduleService, WorkingScheduleService>();
builder.Services.AddScoped<IHolidayService, HolidayService>();
builder.Services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();
builder.Services.AddSingleton<IAdminNotificationPublisher, SignalRAdminNotificationPublisher>();

// Register infrastructure services with interfaces
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));
builder.Services.AddScoped<IEmailService, EmailService>();

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CoffeeApp";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CoffeeAppUsers";

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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (string.IsNullOrEmpty(context.Token))
            {
                context.Token = context.Request.Cookies[authTokenCookieName];
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            var blacklistService = context.HttpContext.RequestServices.GetRequiredService<ITokenBlacklistService>();
            var tokenId = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (!string.IsNullOrWhiteSpace(tokenId) && await blacklistService.IsTokenIdBlacklistedAsync(tokenId))
            {
                context.Fail("Token has been revoked.");
                context.HttpContext.Response.Cookies.Delete(authTokenCookieName, new CookieOptions
                {
                    Path = "/",
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    HttpOnly = true
                });
            }
        },
        OnAuthenticationFailed = async context =>
        {
            var token = context.Request.Cookies[authTokenCookieName];

            if (!string.IsNullOrWhiteSpace(token) && context.Exception is SecurityTokenExpiredException)
            {
                var blacklistService = context.HttpContext.RequestServices.GetRequiredService<ITokenBlacklistService>();
                await blacklistService.BlacklistTokenAsync(token);
            }

            context.HttpContext.Response.Cookies.Delete(authTokenCookieName, new CookieOptions
            {
                Path = "/",
                Secure = true,
                SameSite = SameSiteMode.Strict,
                HttpOnly = true
            });
        },
        OnChallenge = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            context.HandleResponse();

            var requestPath = context.Request.Path.HasValue ? context.Request.Path.Value! : "/Admin/Index";
            var requestQuery = context.Request.QueryString.HasValue ? context.Request.QueryString.Value! : string.Empty;
            var returnUrl = Uri.EscapeDataString($"{requestPath}{requestQuery}");

            context.Response.Redirect($"/Admin/Index?returnUrl={returnUrl}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession(); // Enable session middleware

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Map Razor Pages first (for main UI)
app.MapRazorPages();

// Map API controllers (for /api/* endpoints)
app.MapControllers();
app.MapHub<AdminNotificationHub>("/hubs/admin-notifications");

app.Run();
