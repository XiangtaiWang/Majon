using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

namespace Server;

public class GameServer : IGameServer
{
    // private static List<GameRoom> _roomsList = new();
    private static List<Player> _players = new();
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
        
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(2));
        var lobbyInformation = new LobbyInformation();
        
        while (await timer.WaitForNextTickAsync())
        {
            Console.WriteLine("Broadcasting room list");
            var players = _players.Where(p=> p.GetCurrentRoom()==null);
            lobbyInformation.Rooms = _roomsList.Keys.ToList();
            var info = JsonConvert.SerializeObject(lobbyInformation);
            Console.WriteLine(info);
            var message = Encoding.ASCII.GetBytes(info);  

            foreach (var player in players)
            {
                await player.GetWebSocket().SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true,
                    CancellationToken.None);
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
        _players.Add(player);
        return player;
    }

    public void PlayerLeave(int playerId)
    {
        //todo?
        // var player = _players.First(p=> p.GetPlayerId()==playerId);
        _emptyPlayerId.Enqueue(playerId);
    }

    public void PlayerAction(Player player, string message)
    {
        // var actionType = (PlayerActionType) short.Parse(messageParts[0]);
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
        foreach (var player in _players)
        {
            await WebSocketHelper.SendMessage(player.GetWebSocket(), message);
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

public class LobbyInformation
{
    public readonly PlayerLocation PlayerLocation = PlayerLocation.Lobby;
    public IEnumerable<int> Rooms;
}

public enum PlayerLocation
{
    Unknown = 0,
    Lobby = 1,
    Room = 2
}