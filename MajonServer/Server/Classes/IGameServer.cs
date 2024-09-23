using System.Net.Sockets;
using System.Net.WebSockets;

namespace Server;

public interface IGameServer
{
    IGameRoom CreateRoom(Player player);
    Player AddNewPlayer(WebSocket webSocket);
    void PlayerLeave(int playerId);
    void PlayerAction(Player player, string messageParts);
    Task BroadcastToAll(string message);
    Task BroadcastPlayers(string message, IEnumerable<Player> players);
}