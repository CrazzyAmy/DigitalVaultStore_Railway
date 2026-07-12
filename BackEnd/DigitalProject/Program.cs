using DigitalProject.Data;
using DigitalProject.Hubs;
using DigitalProject.Interface;
using DigitalProject.Interface.Auth;
using DigitalProject.Interface.Category;
using DigitalProject.Interface.Orders;
using DigitalProject.Interface.Payment;
using DigitalProject.Interface.Prouduct;
using DigitalProject.Interface.Reviews;
using DigitalProject.Interface.Role;
using DigitalProject.Interface.User;
using DigitalProject.Middleware;
using DigitalProject.Repositories;
using DigitalProject.Repositories.Payment;
using DigitalProject.Repositories.Prouduct;
using DigitalProject.Repositories.Reviews;
using DigitalProject.Repositories.Role;
using DigitalProject.Security;
using DigitalProject.Services;
using DigitalProject.Services.Payment;
using DigitalProject.Services.Prouduct;
using DigitalProject.Services.Reviews;
using DigitalProject.Services.User;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<DigitalVaultStoreDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DbContext")));

// ── Repositories ──────────────────────────────────────────────────────────────
builder.Services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPaymentServie, PaymentService>();

// ── SignalR ───────────────────────────────────────────────
builder.Services.AddSignalR();

// ── Redis ─────────────────────────────────────────────────
var redisConnectionString = builder.Configuration["Redis:ConnectionString"];
var redisOptions = ConfigurationOptions.Parse(redisConnectionString!);
redisOptions.AbortOnConnectFail = false;
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisOptions));
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = CacheService.InstanceName;
});
builder.Services.AddScoped<ICacheService, CacheService>();

// ── Security ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IJwtHelper, JwtHelper>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// ── CORS ──────────────────────────────────────────────────────────────────────
// [修改] 加入 AllowCredentials()，HttpOnly Cookie 跨域傳輸必須
// [修改] 區分 Dev / Production 環境，Production 讀 appsettings
builder.Services.AddCors(options =>
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins(
                    "http://localhost:5173",
                    "https://localhost:5173"
                  )
                  .AllowCredentials()   // ← 新增：Cookie 跨域必須
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(
                    builder.Configuration["Cors:AllowedOrigin"]
                        ?? throw new InvalidOperationException("Cors:AllowedOrigin is not configured.")
                  )
                  .AllowCredentials()   // ← 新增：Cookie 跨域必須
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    }));

// ── Cookie Policy ─────────────────────────────────────────────────────────────
// [新增] 跨域 HttpOnly Cookie 全域安全規則
// SameSite=None 是跨域 Cookie 的硬性要求，瀏覽器預設 Lax 會直接擋掉
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
    options.Secure = builder.Environment.IsProduction()  // ← 改成 IsProduction
        ? CookieSecurePolicy.Always        // Production 強制 https
        : CookieSecurePolicy.SameAsRequest; // Dev / Testing 允許 http
});

// ── JWT ───────────────────────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtTokenSettings");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = "Cookies";
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.IncludeErrorDetails = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        RequireExpirationTime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
                                       Encoding.UTF8.GetBytes(jwtSettings["IssuerSigningKey"]!)),
        ClockSkew = TimeSpan.Zero,
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // 從 Cookie 中讀取 JWT Token
            var accessToken = context.Request.Cookies["access_token"];
            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 401;
            var body = new
            {
                error = "Unauthorized",
                error_description = "Authentication failed",
            };
            return context.Response.WriteAsync(JsonSerializer.Serialize(body));
        }
    };
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    options.CallbackPath = "/signin-google";
    options.SignInScheme = "Cookies";
});

// ── Authorization ─────────────────────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("admin"));
    options.AddPolicy("CanManageProduct", p => p.RequireRole("admin", "manager"));
    options.AddPolicy("CanViewOrders", p => p.RequireRole("admin", "support"));
    options.AddPolicy("CanManagePayment", p => p.RequireRole("admin", "support"));
    options.AddPolicy("CanManageUser", p => p.RequireRole("admin"));
    options.AddPolicy("CanManageReview", p => p.RequireRole("admin", "support"));
});

// ── Controllers + Swagger ─────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DigitalProject API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "輸入 JWT Token，格式：Bearer {token}",
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
        }] = []
    });
});

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

// 讓 App 知道自己在 HTTPS 反向代理(Railway)後面
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor 
                     | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto,
    // Railway 的代理,清空這兩個限制才會生效
    KnownNetworks = { },
    KnownProxies = { }
});


// ── 啟動時自動套用 EF Core Migration(建立資料表)──
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DigitalVaultStoreDbContext>();
    // InMemory（整合測試）不是關聯式資料庫，Migrate 會擲例外，須跳過
    if (db.Database.IsRelational())
        db.Database.Migrate();
}


// GlobalExceptionMiddleware 最外層，捕捉所有未處理例外
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// ── 靜態檔案 ──────────────────────────────────────────────
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // 快取 7 天
        ctx.Context.Response.Headers.Append(
            "Cache-Control", "public,max-age=604800");
    }
});

// ⚠️ Pipeline 順序硬性規定：
// CORS → CookiePolicy → Authentication → TokenBlacklist → Authorization → Controllers

app.UseCors("AllowFrontend");

// [新增] Cookie 全域安全策略，必須在 UseAuthentication 之前
// 確保所有 Set-Cookie 都套用 HttpOnly / Secure / SameSite=None
app.UseCookiePolicy();

app.UseAuthentication();

// TokenBlacklist 在 Authentication 之後、Authorization 之前
// 確保 JWT 已解析完才做黑名單比對
app.UseMiddleware<TokenBlacklistMiddleware>();

app.UseAuthorization();
app.MapHub<AdminNotificationHub>("/hubs/admin-notification");

app.MapControllers();
app.Run();

public partial class Program { }
