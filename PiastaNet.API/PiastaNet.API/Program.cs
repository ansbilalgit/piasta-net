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
<!doctype html>
<html>
<head>
  <meta charset="utf-8" />
  <title>Piasta API</title>
  <style>
    body { font-family: Arial, sans-serif; margin: 40px; }
    a { font-weight: bold; }
  </style>
</head>
<body>
  <h2>Hello and welcome to Piasta API</h2>
  <p>
    <a href="/swagger">Click here to see swagger documentation</a>
  </p>
</body>
</html>
""";
    return Results.Content(html, "text/html");
});

// ---- Controllers routes
app.MapControllers();

app.Run();
