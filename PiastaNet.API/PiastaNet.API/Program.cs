using Microsoft.EntityFrameworkCore;
using PiastaNet.API.Data;
using PiastaNet.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IItemsService, ItemsService>();
builder.Services.AddScoped<ILibraryTypeService, LibraryTypeService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

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
