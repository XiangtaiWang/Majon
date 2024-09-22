using System.Net.WebSockets;
using System.Text;

namespace Server;

public static class WebSocketHelper{

    public static async Task SendMessage(WebSocket connection, string message)
    {
        
        if (connection.State == WebSocketState.Open)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await connection.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        else
        {
            throw new Exception("no connection");
        }
    }
}