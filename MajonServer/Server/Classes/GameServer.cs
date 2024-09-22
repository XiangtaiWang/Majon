using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

namespace Server;

public class GameServer : IGameServer
{
    // private static List<GameRoom> _roomsList = new();
    private static List<Player> _players = new();
    private static Queue<int> _emptyPlayerId = new();
    private static Queue<int> _emptyRoom = new();
    private static ConcurrentDictionary<int, IGameRoom> _roomsList = new();

    public GameServer()
    {
        for (int i = 0; i < 100; i++)
        {
            _emptyPlayerId.Enqueue(i);
            _emptyRoom.Enqueue(i);
        } 
    }
    
    public IGameRoom CreateRoom(Player player)
    {
        var roomId = _emptyRoom.Dequeue();
        var room = new GameRoom(roomId);
        _roomsList.TryAdd(roomId, room);
        room.PlayerJoin(player);
        return room;
    }
    
    public Player AddNewPlayer(WebSocket webSocket)
    {
        var player = new Player(_emptyPlayerId.Dequeue(), webSocket);
        _players.Add(player);
        return player;
    }

    public void PlayerLeave(int playerId)
    {
        //todo?
        // var player = _players.First(p=> p.GetPlayerId()==playerId);
        _emptyPlayerId.Enqueue(playerId);
    }

    public void PlayerAction(Player player, string[] messageParts)
    {
        // var actionType = (PlayerActionType) short.Parse(messageParts[0]);
        var playerCurrentRoom = player.GetCurrentRoom();
        if (playerCurrentRoom == null)
        {
            HandleLobbyMessage(player, messageParts);
        }
        else
        {
            playerCurrentRoom.HandleRoomMessage(player, messageParts);
        }
        
    }

    private void HandleLobbyMessage(IPlayer player, string[] messageParts)
    {
        var action = short.Parse(messageParts[0]);
        var playerRoomAction = (PlayerInLobbyAction)action;
        int roomId;
        switch (playerRoomAction)
        {
            case PlayerInLobbyAction.Create:
                roomId = _emptyRoom.Dequeue();
                var gameRoom = new GameRoom(roomId);
                _roomsList.TryAdd(roomId, gameRoom);
                player.SetRoom(gameRoom);
                break;
            case PlayerInLobbyAction.Join:
                roomId = int.Parse(messageParts[1]);
                if (_roomsList.TryGetValue(roomId, out var room))
                {
                    player.SetRoom(room);    
                }
                
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
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