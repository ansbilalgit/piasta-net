using Microsoft.EntityFrameworkCore;
using PiastaNet.API.Data;

var builder = WebApplication.CreateBuilder(args);

// ---- Logging (helps in Log Stream + App Insights if enabled)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ---- Controllers (NOT minimal APIs)
builder.Services.AddControllers();

// ---- Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

var app = builder.Build();

// ---- Swagger
app.UseSwagger();
app.UseSwaggerUI();

// ---- CORS (place before auth and MapControllers)
app.UseCors(CorsPolicyName);

// ---- Optional: HTTPS redirection
app.UseHttpsRedirection();

// ---- Optional auth
// app.UseAuthentication();
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

app.Run();
