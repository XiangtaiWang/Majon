using System.Net;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:7777");
var app = builder.Build();

// app.MapGet("/", () => "Hello World!");
app.UseWebSockets();
app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        while (true)
        {
            
        }
        
    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }

});
await app.RunAsync();