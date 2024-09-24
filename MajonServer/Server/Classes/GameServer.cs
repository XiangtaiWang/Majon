using System.Collections.Concurrent;
using System.Net.WebSockets;
using Newtonsoft.Json;

namespace Server.Classes;

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
            try
            {
                Console.WriteLine("Broadcasting room list " + DateTime.Now);
                var players = Players.Values.Where(p => p.GetCurrentRoom() == null).ToList();
                lobbyInformation.Rooms = RoomsList.Keys.ToList();
                var info = JsonConvert.SerializeObject(lobbyInformation);

                await BroadcastPlayers(info, players);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in BroadcastRoomList: {ex.Message}");
            }
            
        }
    }

    private static int GetNonUsedRoomId()
    {
        UnusedRoom.TryPop(out var roomId);
        return roomId;
    }

    public Player AddNewPlayer(WebSocket webSocket)
    {
        var playerId = GetUnUsedPlayerId(); 
        while (Players.Values.FirstOrDefault(p=>p.GetPlayerId()==playerId, null) != null)
        {
            playerId = GetUnUsedPlayerId();
        }
        var player = new Player(playerId, webSocket);
        Players.TryAdd(player.GetPlayerId(), player);
        return player;
    }

    private static int GetUnUsedPlayerId()
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

    public async Task PlayerAction(Player player, string message)
    {
        var messageParts = message.Split('|');
        var playerCurrentRoom = player.GetCurrentRoom();
        if (playerCurrentRoom == null)
        {
            Console.WriteLine("handle lobby message");
            await HandleLobbyMessage(player, messageParts);
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

    private async Task HandleLobbyMessage(Player player, string[] messageParts)
    {
        var action = short.Parse(messageParts[0]);
        var playerRoomAction = (PlayerInLobbyAction)action;
        GameRoom room;
        switch (playerRoomAction)
        {
            case PlayerInLobbyAction.Create:
                room = PlayerCreateRoom();
                await PlayerJoinRoom(player, room);
                break;
            case PlayerInLobbyAction.Join:
                var roomId = int.Parse(messageParts[1]);
                if (RoomsList.TryGetValue(roomId, out room))
                {
                    if (!room.IsGameRunning)
                    {
                        await PlayerJoinRoom(player, room);
                    }
                    else
                    {
                        Console.WriteLine($"Room{room} already started.");    
                    }
                    
                }
                else
                {
                    Console.WriteLine($"Room{room} not found.");
                }
                break;
        }
        
    }

    private GameRoom PlayerCreateRoom()
    {
        int roomId;
        GameRoom room;
        roomId = GetNonUsedRoomId();
        room = new GameRoom(roomId, this);
        RoomsList.TryAdd(roomId, room);
        return room;
    }

    private static async Task PlayerJoinRoom(Player player, GameRoom gameRoom)
    {
        player.SetRoom(gameRoom);
        await gameRoom.PlayerJoin(player);
    }
}