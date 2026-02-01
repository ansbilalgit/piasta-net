var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => Results.Content(@"
<!DOCTYPE html>
<html>
<head>
    <title>Piasta-Net</title>
</head>
<body>
    <h1>Hello!</h1>
    <p>Coming soon!</p>
</body>
</html>", "text/html"));

app.Run();