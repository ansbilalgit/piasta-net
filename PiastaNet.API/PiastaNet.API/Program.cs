using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PiastaNet.API.Data;
using PiastaNet.API.Services;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ---- Logging (helps in Log Stream + App Insights if enabled)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ---- Controllers (NOT minimal APIs)
builder.Services.AddControllers();

// ---- Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // 1. Define the Bearer Auth scheme
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token: Bearer {your_token}"
    });

    // 2. Make Swagger use that scheme
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ---- CORS
const string CorsPolicyName = "PiastaCors";

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        // Simple safe default during dev:
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();

        // If you want credentials (cookies/auth), you must NOT use AllowAnyOrigin.
        // Instead:
        // policy.WithOrigins("https://your-frontend.com").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

builder.Services.AddScoped<IItemsService, ItemsService>();
builder.Services.AddScoped<ILibraryTypeService, LibraryTypeService>();
builder.Services.AddScoped<IGameEventService, GameEventService>();


// ---- DbContext (add EnableRetryOnFailure since your DB is serverless/pauses)
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        ));
});
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Default Password settings.
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;

    // Allow usernames like "admin" (not just emails)
    options.User.RequireUniqueEmail = false;
});
// 2. Setup JWT Authentication
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var payload = new
        {
            error = ex.Message,
            exception = ex.GetType().FullName,
            stackTrace = ex.StackTrace,
            innerException = ex.InnerException?.Message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
});



// ---- Swagger
app.UseSwagger();
app.UseSwaggerUI();

// ---- CORS (place before auth and MapControllers)
app.UseCors(CorsPolicyName);

// ---- Optional: HTTPS redirection
app.UseHttpsRedirection();

// ---- Optional auth
// app.UseAuthentication();
app.UseAuthentication();
app.UseAuthorization();

// ---- Welcome page at "/"
app.MapGet("/", () =>
{
    var html = """
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<title>Piasta API</title>

<style>
    body {
        background-color: #0d1117;
        color: #e6edf3;
        font-family: Consolas, monospace;
        text-align: center;
        padding-top: 80px;
    }

    .container {
        max-width: 700px;
        margin: auto;
        padding: 30px;
        border: 1px solid #30363d;
        border-radius: 12px;
        background: #161b22;
        box-shadow: 0 0 20px rgba(0,255,170,0.15);
    }

    h1 {
        color: #00ffae;
        font-size: 32px;
        margin-bottom: 10px;
    }

    .subtitle {
        color: #8b949e;
        margin-bottom: 30px;
    }

    .btn {
        display: inline-block;
        padding: 12px 22px;
        margin-top: 20px;
        background: #00ffae;
        color: #0d1117;
        text-decoration: none;
        font-weight: bold;
        border-radius: 8px;
        transition: 0.2s;
    }

    .btn:hover {
        background: #00cc8a;
        transform: scale(1.05);
    }

    .console {
        margin-top: 25px;
        text-align: left;
        background: #010409;
        padding: 15px;
        border-radius: 8px;
        border: 1px solid #30363d;
        color: #7ee787;
        font-size: 14px;
    }
</style>
</head>

<body>

<div class="container">
    <h1>🎮 PIastaNet API</h1>
    <div class="subtitle">
        Welcome, Developer.<br/>
        Backend systems initialized successfully.
    </div>

    <a class="btn" href="/swagger">⚡ Enter Swagger Console</a>

    <div class="console">
        > Loading Game Engine... ✔️ <br/>
        > Database Connection... ✔️ <br/>
        > Controllers Registered... ✔️ <br/>
        > Ready for API Requests 🚀
    </div>
</div>

</body>
</html>
""";

    return Results.Content(html, "text/html");
});


// ---- Controllers routes
app.MapControllers();

// Migrate + Seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    var sqlitePathFromConfig = builder.Configuration.GetConnectionString("SqliteSeedPath") ?? "database.sqlite";
    var sqlitePath = Path.IsPathRooted(sqlitePathFromConfig)
        ? sqlitePathFromConfig
        : Path.Combine(AppContext.BaseDirectory, sqlitePathFromConfig);
    await SqliteSeeder.SeedFromSqliteAsync(db, sqlitePath);
}
app.Run();
