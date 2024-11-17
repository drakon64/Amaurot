var builder = WebApplication.CreateSlimBuilder();
var app = builder.Build();
app.MapGet("/healthcheck", () => Results.Ok());
app.Run($"http://*:{Environment.GetEnvironmentVariable("PORT")}");
