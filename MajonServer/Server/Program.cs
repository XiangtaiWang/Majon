using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

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

            // 如果请求的是 WebSocket 升级
            if (context.Request.IsWebSocketRequest)
            {
                // 接受 WebSocket 连接
                WebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                WebSocket webSocket = wsContext.WebSocket;

                new Thread((() => HandlePlayerConnection(webSocket, gameServer))).Start();
                Console.WriteLine("Client connected!");

                // 开启一个任务来处理客户端

            }
            else
            {
                // 不是 WebSocket 请求，返回 400 Bad Request
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }

        // while (true) // Add your exit flag here
        // {
        //
        //     var client = await server.AcceptTcpClientAsync();
        //     Console.WriteLine("a client connected");
        //     new Thread(() => _ = HandlePlayerConnection(client, gameServer)).Start();
        //
        // }

    }

    private static async Task HandlePlayerConnection(WebSocket webSocket, IGameServer gameServer)
    {
        // NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        var player = gameServer.AddNewPlayer(webSocket);

        
        while (webSocket.State == WebSocketState.Open)
        {
            // 接收客户端数据
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                // 客户端请求关闭连接
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
                Console.WriteLine("Client disconnected.");
                gameServer.PlayerLeave(player.GetPlayerId());
            }
            else
            {

                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Received: {receivedMessage}");
                var response = Encoding.UTF8.GetBytes($"Server: hihi received");
                await webSocket.SendAsync(new ArraySegment<byte>(response), WebSocketMessageType.Text, true, CancellationToken.None);
                // 发送消息给所有客户端
                // await BroadcastMessageAsync(receivedMessage);
            }
        }
        
        // while (client.Connected)
        // {
        //     var bytes = await stream.ReadAsync(buffer);
        //     var message = Encoding.ASCII.GetString(buffer, 0, bytes);
        //     if (IsWebSocketHandShake(message))
        //     {
        //         Console.WriteLine("hand shake");
        //         continue;
        //     }
        //
        //     Console.WriteLine(message);
        //     var messageParts = message.Split('|');
        //     gameServer.PlayerAction(player, messageParts);
        //
        // }
        // Console.WriteLine(player.GetPlayerId()+" disconnected");
        
    }

    private static bool IsWebSocketHandShake(string message)
    {
        return message.Contains("Upgrade: websocket");
    }
}