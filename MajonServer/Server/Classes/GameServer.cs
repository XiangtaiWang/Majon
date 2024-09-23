using System.Collections.Concurrent;
using System.Net.WebSockets;
using Newtonsoft.Json;

namespace Server;

public class GameServer : IGameServer
{
    // private static List<GameRoom> _roomsList = new();
    private static readonly ConcurrentDictionary<int, Player> Players = new();
    private static readonly ConcurrentStack<int> UnUsedPlayerId = new();
    private static readonly ConcurrentStack<int> UnusedRoom = new();
    private static readonly ConcurrentDictionary<int, GameRoom> RoomsList = new();

    public GameServer()
    {
        for (int i = 100; i >0; i--)
        {
            UnUsedPlayerId.Push(i);
            UnusedRoom.Push(i);
        } 
        
        new Thread((() => _ = BroadcastRoomList())).Start();
    }

    private async Task BroadcastRoomList()
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(5));
        var lobbyInformation = new LobbyInformation();

        while (await timer.WaitForNextTickAsync())
        {
            Console.WriteLine("Broadcasting room list" + DateTime.Now);
            var players = Players.Values.Where(p => p.GetCurrentRoom() == null).ToList();
            lobbyInformation.Rooms = RoomsList.Keys.ToList();
            var info = JsonConvert.SerializeObject(lobbyInformation);

            await BroadcastPlayers(info, players);
            
        }
    }

    public IGameRoom CreateRoom(Player player)
    {
        var roomId = GetNonUsedRoomId();
        var room = new GameRoom(roomId);
        RoomsList.TryAdd(roomId, room);
        room.PlayerJoin(player);
        return room;
    }

    private static int GetNonUsedRoomId()
    {
        UnusedRoom.TryPop(out var roomId);
        return roomId;
    }

    public Player AddNewPlayer(WebSocket webSocket)
    {
        var playerId = GetNonUsedPlayerId();
        var player = new Player(playerId, webSocket);
        Players.TryAdd(player.GetPlayerId(), player);
        return player;
    }

    private static int GetNonUsedPlayerId()
    {
        UnUsedPlayerId.TryPop(out var playerId);
        return playerId;
    }

    public void PlayerLeave(int playerId)
    {
        if (!Players.TryRemove(playerId, out var player))
        {
            Console.WriteLine($"Player with ID {playerId} not found in _players dictionary.");
        }
        UnUsedPlayerId.Push(playerId);
        
        var currentRoom = player.GetCurrentRoom();
        currentRoom.PlayerLeave(player);
        if (currentRoom.PlayerCount==0)
        {
            var roomId = currentRoom.GetRoomId();
            RoomsList.TryRemove(roomId, out _);
            UnusedRoom.Push(roomId);
        }
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
        await BroadcastPlayers(message, Players.Values);
    }

    public async Task BroadcastPlayers(string message, IEnumerable<Player> players)
    {
        foreach (var player in players)
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
                roomId = GetNonUsedRoomId();
                var gameRoom = new GameRoom(roomId);
                RoomsList.TryAdd(roomId, gameRoom);
                player.SetRoom(gameRoom);
                break;
            case PlayerInLobbyAction.Join:
                roomId = int.Parse(messageParts[1]);
                if (RoomsList.TryGetValue(roomId, out var room))
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

public class PlayerJoinedInfo(string message)
{
    public readonly MessageType MessageType = MessageType.CanIgnore;
    public string Message = message;
}
public class LobbyInformation
{
    public readonly MessageType MessageType = MessageType.Lobby;
    public IEnumerable<int> Rooms;
}
public class RoomInformation(string message)
{
    public readonly MessageType MessageType = MessageType.Room;
    public string Message = message;
}
public enum MessageType
{
    CanIgnore = 0,
    Lobby = 1,
    Room = 2
}