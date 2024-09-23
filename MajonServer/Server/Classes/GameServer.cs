using System.Collections.Concurrent;
using System.Net.WebSockets;
using Newtonsoft.Json;

namespace Server;

public class GameServer : IGameServer
{
    // private static List<GameRoom> _roomsList = new();
    private static ConcurrentDictionary<int, Player> _players = new();
    private static Queue<int> _emptyPlayerId = new();
    private static Queue<int> _emptyRoom = new();
    private static ConcurrentDictionary<int, GameRoom> _roomsList = new();

    public GameServer()
    {
        for (int i = 0; i < 100; i++)
        {
            _emptyPlayerId.Enqueue(i);
            _emptyRoom.Enqueue(i);
        } 
        
        new Thread((() => BroadcastRoomList())).Start();
    }

    private async Task BroadcastRoomList()
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(10));
        var lobbyInformation = new LobbyInformation();

        while (await timer.WaitForNextTickAsync())
        {
            Console.WriteLine("Broadcasting room list" + DateTime.Now);
            var players = _players.Values.Where(p => p.GetCurrentRoom() == null).ToList();
            lobbyInformation.Rooms = _roomsList.Keys.ToList();
            var info = JsonConvert.SerializeObject(lobbyInformation);

            foreach (var player in players)
            {
                try
                {
                    if (player.GetWebSocket().State == WebSocketState.Open)
                    {
                        await WebSocketHelper.SendMessage(player.GetWebSocket(), info);
                    }
                    else
                    {
                        PlayerLeave(player.GetPlayerId());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception sending message to player {player.GetPlayerId()}: {ex.Message}");
                    PlayerLeave(player.GetPlayerId());
                }
            }
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
        _players.TryAdd(player.GetPlayerId(), player);
        return player;
    }

    public void PlayerLeave(int playerId)
    {
        _players.TryRemove(playerId, out var player);
        _emptyPlayerId.Enqueue(playerId);
    }

    public void PlayerAction(Player player, string message)
    {
        var messageParts = message.Split('|');
        var playerCurrentRoom = player.GetCurrentRoom();
        if (playerCurrentRoom == null)
        {
            Console.WriteLine("handle lobby message");
            HandleLobbyMessage(player, messageParts);
        }
        else
        {
            Console.WriteLine("handle room message");
            playerCurrentRoom.HandleRoomMessage(player, messageParts);
        }
        
    }

    public async Task BroadcastToAll(string message)
    {
        foreach (var player in _players.Values)
        {
            try
            {
                if (player.GetWebSocket().State == WebSocketState.Open)
                {
                    await WebSocketHelper.SendMessage(player.GetWebSocket(), message);
                }
                else
                {
                    PlayerLeave(player.GetPlayerId());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception sending message to player {player.GetPlayerId()}: {ex.Message}");
                PlayerLeave(player.GetPlayerId());
            }
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
            // todo: send message 
        }
    }
}

public class PlayerJoinedInfo
{
    public readonly MessageType MessageType = MessageType.CanIgnore;
    public string Message;
}
public class LobbyInformation
{
    public readonly MessageType MessageType = MessageType.Lobby;
    public IEnumerable<int> Rooms;
}

public enum MessageType
{
    CanIgnore = 0,
    Lobby = 1,
    Room = 2
}