using System.Text.Json;
using Amaurot.Common.Models;

var builder = WebApplication.CreateSlimBuilder();
var app = builder.Build();

app.MapGet("/", () => Results.Ok());

app.MapPost(
    "/plan",
    (TaskRequestBody taskRequestBody) =>
        Results.Ok(
            JsonSerializer.Serialize(
                taskRequestBody,
                TaskRequestBodyJsonSerializerContext.Default.TaskRequestBody
            )
        )
);

app.Run($"http://*:{Environment.GetEnvironmentVariable("PORT")}");
