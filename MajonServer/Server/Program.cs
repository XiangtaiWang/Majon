using System.Net;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Server.Classes;

namespace Server;

class MajonServer
{
    public static async Task Main(string[] args)
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IGameServer, GameServer>()
            .BuildServiceProvider();

        var gameServer = serviceProvider.GetService<IGameServer>();
        
        // TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 7777);
        HttpListener httpListener = new HttpListener();
        httpListener.Prefixes.Add("http://localhost:7777/ws/");
        httpListener.Start();
        while (true)
        {
            HttpListenerContext context = await httpListener.GetContextAsync();
            
            if (context.Request.IsWebSocketRequest)
            {
                // 接受 WebSocket 连接
                WebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                WebSocket webSocket = wsContext.WebSocket;
                
                new Thread((() => _ = HandlePlayerConnection(webSocket, gameServer))).Start();
                Console.WriteLine("Client connected!");

            }
            else
            {
                // 不是 WebSocket 请求，返回 400 Bad Request
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }
        
    }

    private static async Task HandlePlayerConnection(WebSocket webSocket, IGameServer gameServer)
    {
        // NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        var player = gameServer.AddNewPlayer(webSocket);
        var playerInfo = JsonConvert.SerializeObject(new PlayerInfo(player.GetPlayerId()));

        await gameServer.BroadcastPlayers(playerInfo, new List<Player>() { player });
        var notification = $"player{player.GetPlayerId()} joined";
        var notImportantInfo = new NotImportantInfo(notification);
        var message = JsonConvert.SerializeObject(notImportantInfo);
        await gameServer.BroadcastToAll(message);
        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
                    Console.WriteLine("Client disconnected.");
                    gameServer.PlayerLeave(player.GetPlayerId());
                    break;
                }
                else
                {
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await gameServer.PlayerAction(player, receivedMessage);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in HandlePlayerConnection: {ex.Message}");
            gameServer.PlayerLeave(player.GetPlayerId());
        }
    }
}