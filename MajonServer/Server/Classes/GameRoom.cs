using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using Server.Classes;

namespace Server;

public class GameRoom : IGameRoom
{
    private List<Player> _players;
    private List<Tile> _allTiles;
    private ConcurrentStack<Tile> _sentTiles;
    public int PlayerCount => _players.Count;
    private int _roomId;
    public bool IsGameRunning = false;
    private ConcurrentDictionary<SeatDirection, Player> _seatInfo = new();
    private short _initialTurnIndex = 1;
    private LinkedListNode<SeatDirection> _currentTurnIndex;
    private GameServer _server;
    public ConcurrentQueue<ActionRequest> ActionRequestsQueue;
    
        
    private readonly List<TileType> _numberTiles = new List<TileType>
    { 
        TileType.One,
        TileType.Tiao,
        TileType.Tong
    };

    private readonly List<TileType> _letterTiles = new List<TileType>
    { 
        TileType.RedCenter,
        TileType.EarnMoney,
        TileType.WhiteBoard,
        TileType.EastWind,
        TileType.SouthWind,
        TileType.WestWind,
        TileType.NorthWind
    };

    private TurnIndex _turnIndexs;
    private RoomInformation _roomInformation;

    public GameRoom(int roomId, GameServer gameServer)
    {
        _roomId = roomId;
        _players = new List<Player>();
        _allTiles = new List<Tile>();
        ActionRequestsQueue = new ConcurrentQueue<ActionRequest>();
        _server = gameServer;
        _roomInformation = new RoomInformation()
        {
            RoomId = roomId
        };
    }

    private void AllocateTiles(Random random)
    {
        _allTiles = _allTiles.OrderBy(x => random.Next()).ToList();
        foreach (var player in _players)
        {
            player.SetHandTiles(_allTiles[..16]);
            player.HandTiles = player.HandTiles.OrderBy(t=>t.TileType).ThenBy(t=>t.TileNumber).ToList();
            _allTiles.RemoveRange(0, 16);
        }
    }

    private void CreateTiles()
    {
        _sentTiles = new ConcurrentStack<Tile>();
        foreach (var numberTile in _numberTiles)
        {
            for (var i = 1; i <= 4; i++)
            {
                for (short j = 1; j <= 9; j++)
                {
                    _allTiles.Add(new Tile(tileType: numberTile, tileNumber: j));
                }   
            }
        }
        foreach (var letterTile in _letterTiles)
        {
            for (short i = 1; i <= 9; i++)
            {
                _allTiles.Add(new Tile(tileType: letterTile, tileNumber:0));
            }
        }
    }
    public async Task StartGame()
    {
        Console.WriteLine("Game Start!!");
        await BroadcastToRoomPlayers(JsonConvert.SerializeObject(new NotImportantInfo("Game Start!!")));
        try
        {
            _roomInformation.Players = GetPlayers();
            _roomInformation.PlayersInfo = _players;
            CreateTiles();
            var random = new Random();
            _turnIndexs = new TurnIndex();
            _currentTurnIndex = _turnIndexs.Find(SeatDirection.East);
        
            AllocateTiles(random);
            AllocateSeats(random);
        
            await RoundStart();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }

    private async Task RoundStart()
    {
        _seatInfo.TryGetValue(_currentTurnIndex.Value, out var thisTurnPlayer);
        thisTurnPlayer.IsThisPlayerTurn = true;

        await BroadcastRoomInfo();
    }

    private async Task BroadcastRoomInfo()
    {
        var msg = JsonConvert.SerializeObject(_roomInformation);
        // Console.WriteLine(msg);
        await BroadcastToRoomPlayers(msg);
    }

    private async Task BroadcastToRoomPlayers(string msg)
    {
        await _server.BroadcastPlayers(msg, _players);
    }

    private void AllocateSeats(Random random)
    {
        _players = _players.OrderBy(x => random.Next()).ToList();
        var seatDirections = new List<SeatDirection>()
        {
            SeatDirection.East,
            SeatDirection.South,
            SeatDirection.West,
            SeatDirection.North,
        };
        _seatInfo.Clear();
        foreach (var direction in seatDirections)
        {
            var player = _players[seatDirections.IndexOf(direction)];
            player.Seat = direction;
            _seatInfo.TryAdd(direction, player);
        }
        
        // throw new NotImplementedException();
    }

    public void WinThisRound()
    {
        //TODO

    }
    

    public void GameFinish()
    {
        throw new NotImplementedException();
    }
    

    public async Task PlayerJoin(Player player)
    {
        _players.Add(player);
        var roomInformation = new RoomInformation
        {
            RoomId = _roomId,
            Players = GetPlayers()
        };
        var msg = JsonConvert.SerializeObject(roomInformation);
        await BroadcastToRoomPlayers(msg);
        if (_players.Count==4)
        {
            await StartGame();
        }
    }

    public void PlayerLeave(Player player)
    {
        _players = _players.Where(p => p.GetPlayerId() != player.GetPlayerId()).ToList();
    }

    public List<Player> IsPlayerIdInThisRoom()
    {
        return _players;
    }

    public async Task HandleRoomMessage(Player player, string[] messageParts)
    {
        var actionType = (PlayerInRoomAction)int.Parse(messageParts[0]);
        //
        if (player.IsThisPlayerTurn)
        {
            switch (actionType)
            {
                case PlayerInRoomAction.SendTile: 
                    var tileType = (TileType) short.Parse(messageParts[1]);
                    var tileNumber = short.Parse(messageParts[2]);
                    var tile = new Tile(tileType, tileNumber);
                    var actionRequest = new ActionRequest
                    {
                        PlayerActionType = PlayerInRoomAction.SendTile,
                        Player = player,
                        Tile = tile,
                    };
                    ActionRequestsQueue.Enqueue(actionRequest);
                    await ProcessActionRequest();
                    break;
                case PlayerInRoomAction.AskToWin:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }

    public void NewRound()
    {
        _initialTurnIndex++;
        if (_initialTurnIndex>4)
        {
            _initialTurnIndex = 1;
        }
        //todo
    }

    public int GetRoomId()
    {
        return _roomId;
    }

    private async Task ProcessActionRequest()
    {
        while (!ActionRequestsQueue.IsEmpty)
        {
            ActionRequestsQueue.TryDequeue(out var rq);
            ActionRequestsQueue.Clear();
            var player = rq.Player;
            var tile = rq.Tile;
            var playerActionType = rq.PlayerActionType;
            switch (playerActionType)
            {
                case PlayerInRoomAction.SendTile:
                    if (player.HandTiles.Contains(tile))
                    {
                        player.IsThisPlayerTurn = false;
                        player.HandTiles.Remove(tile);
                        player.SentTiles.Push(tile);
                        _sentTiles.Push(tile);
                        await UpdatePlayersStatus(player);
                        await WaitingAction();
                        if (!ActionRequestsQueue.IsEmpty)
                        {
                            continue;
                        }
                        else
                        {
                            var nextPlayer = GetNextPlayer();
                            ActionRequestsQueue.Enqueue(new ActionRequest
                            {
                                PlayerActionType = PlayerInRoomAction.FetchTile,
                                Player = nextPlayer,
                            });
                            ClearOtherPlayerAvailaibleAction();
                            //todo next player fetch tile and send coundown(optional)   
                        }
                    }

                    break;
                case PlayerInRoomAction.FetchTile:
                    player.IsThisPlayerTurn = true;
                    _currentTurnIndex = _turnIndexs.Find(player.Seat);
                    await BroadcastRoomInfo();
                    // todo: fetch
                    break;
                case PlayerInRoomAction.Eat:
                    player.IsThisPlayerTurn = true;
                    _currentTurnIndex = _turnIndexs.Find(player.Seat);
                    await BroadcastRoomInfo();
                    //todo
                    break;
                case PlayerInRoomAction.Pong:
                    player.IsThisPlayerTurn = true;
                    _currentTurnIndex = _turnIndexs.Find(player.Seat);
                    await BroadcastRoomInfo();
                    //todo
                    break;
                case PlayerInRoomAction.AskToWin:
                    //todo
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void ClearOtherPlayerAvailaibleAction()
    {
        foreach (var player in _players)
        {
            player.ClearAvailibleAction();
        }
        
    }

    private async Task WaitingAction()
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));
        var seconds = 5;
        var waitingActionNotification = new WaitingActionNotification();
        while (await timer.WaitForNextTickAsync())
        {
            try
            {
                waitingActionNotification.LeftSeconds = seconds;
                Console.WriteLine("Broadcasting room list " + DateTime.Now); ;
                var info = JsonConvert.SerializeObject(waitingActionNotification);
                await BroadcastToRoomPlayers(info);
                seconds--;
                if (seconds<0 || !ActionRequestsQueue.IsEmpty)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in countdown timer: {ex.Message}");
            }
        }
    }

    private async Task UpdatePlayersStatus(Player player)
    {
        // _seatInfo.TryGetValue(_currentTurnIndex.Value, out var player);
        var nextPlayer = GetNextPlayer();

        // nextPlater.IsThisPlayerTurn = true;
        
        player.SentTiles.TryPeek(out var lastTile);
        nextPlayer.EatCheck(lastTile);
        foreach (var otherPlayer in _players.Where(p=>p.GetPlayerId()!=player.GetPlayerId()))
        {
            otherPlayer.PongCheck(lastTile);    
        }
        
        await BroadcastRoomInfo();
    }

    private Player? GetNextPlayer()
    {
        // _currentTurnIndex = _turnIndexs.GoNext(_currentTurnIndex.Value);
        var next = _turnIndexs.GoNext(_currentTurnIndex.Value);
        _seatInfo.TryGetValue(next.Value, out var nextPlayer);
        return nextPlayer;
    }

    public List<int> GetPlayers()
    {
        return _players.Select(p=>p.GetPlayerId()).ToList();
    }
}

internal class WaitingActionNotification
{
    public readonly MessageType MessageType = MessageType.WaitingAction;
    public int LeftSeconds;
}

public class ActionRequest
{
    public PlayerInRoomAction PlayerActionType;
    public Player Player;
    public Tile Tile;
}
