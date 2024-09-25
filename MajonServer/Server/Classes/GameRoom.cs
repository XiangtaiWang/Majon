using System.Collections.Concurrent;
using Newtonsoft.Json;
using Server.Classes;

namespace Server;

public class GameRoom : IGameRoom
{
    private List<Player> _players;
    private List<Tile> _allTiles;
    public int PlayerCount => _players.Count;
    private int _roomId;
    public bool IsRoomFull =>PlayerCount>=4;
    private ConcurrentDictionary<SeatDirection, Player> _seatInfo = new();
    private short _initialTurnIndex = 1;
    private LinkedListNode<SeatDirection> _currentTurnIndex;
    private GameServer _server;
    private readonly ConcurrentQueue<ActionRequest> _actionRequestsQueue;
    private readonly ConcurrentStack<ActionRequest> _actionHistory;
    
        
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
        _actionRequestsQueue = new ConcurrentQueue<ActionRequest>();
        _actionHistory = new ConcurrentStack<ActionRequest>();
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
            player.SortHandTiles();
            _allTiles.RemoveRange(0, 16);
        }
    }

    private void CreateTiles()
    {
        new ConcurrentStack<Tile>();
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
            for (short i = 1; i <=4; i++)
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
        var actionRequest = new ActionRequest
        {
            PlayerActionType = PlayerInRoomAction.FetchTile,
            Player = thisTurnPlayer,
        };
        _actionRequestsQueue.Enqueue(actionRequest);
        await ProcessActionRequest();
        // thisTurnPlayer.IsThisPlayerTurn = true;

        // await BroadcastRoomInfo();
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
        
    }

    public async Task WinThisRound(Player player)
    {
        var roomAnnouncement = new RoomAnnouncement
        {
            RoomId = _roomId,
            WinnerPlayerId =  player.GetPlayerId(),
            PlayersInfo = _players
        };
        var msg = JsonConvert.SerializeObject(roomAnnouncement);
        await BroadcastToRoomPlayers(msg);
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
        var actionRequest = new ActionRequest();
        switch (actionType)
            {
                case PlayerInRoomAction.SendTile:
                    if (player.IsThisPlayerTurn)
                    {
                        var tileType = (TileType)int.Parse(messageParts[1]);
                        var tileNumber = int.Parse(messageParts[2]);
                        var tile = new Tile(tileType, tileNumber);
                        actionRequest.PlayerActionType = PlayerInRoomAction.SendTile;
                        actionRequest.Player = player;
                        actionRequest.Tiles = [tile];
                        
                    }

                    break;
                case PlayerInRoomAction.AskToWin:
                    actionRequest.PlayerActionType = PlayerInRoomAction.AskToWin;
                    actionRequest.Player = player;
                    break;
                case PlayerInRoomAction.Eat:
                    actionRequest.PlayerActionType = PlayerInRoomAction.Eat;
                    actionRequest.Player = player;
                    var tile1 = new Tile((TileType)int.Parse(messageParts[1]), int.Parse(messageParts[2]));
                    var tile2 = new Tile((TileType)int.Parse(messageParts[3]), int.Parse(messageParts[4]));
                    actionRequest.Tiles = [tile1, tile2];
                    break;
                case PlayerInRoomAction.Pong:
                    actionRequest.PlayerActionType = PlayerInRoomAction.Pong;
                    actionRequest.Player = player;
                    break;
                case PlayerInRoomAction.Gang:
                    actionRequest.PlayerActionType = PlayerInRoomAction.Gang;
                    actionRequest.Player = player;
                    break;
                // throw new ArgumentOutOfRangeException();
            
            }
        _actionRequestsQueue.Enqueue(actionRequest);
        await ProcessActionRequest();

    }

    public void NewRound()
    {
        _initialTurnIndex++;
        if (_initialTurnIndex>4)
        {
            _initialTurnIndex = 1;
        }
    }

    public int GetRoomId()
    {
        return _roomId;
    }

    private async Task ProcessActionRequest()
    {
        while (!_actionRequestsQueue.IsEmpty)
        {
            _actionRequestsQueue.TryDequeue(out var rq);
            _actionRequestsQueue.Clear();
            var player = rq.Player;
            ActionRequest lastRq;
            Tile lastTile;
            var playerActionType = rq.PlayerActionType;
            ClearAllPlayerAvailableAction();
            switch (playerActionType)
            {
                case PlayerInRoomAction.SendTile:
                    var tile = rq.Tiles.First();
                    if (player.HandTiles.Contains(tile))
                    {
                        player.IsThisPlayerTurn = false;
                        player.HandTiles.Remove(tile);
                        player.SentTiles.Push(tile);
                        await UpdatePlayersStatus(player);
                        
                        var actionHistoryCountBeforeWaiting = _actionHistory.Count;
                        if (_players.Any(p => p.AvailableActions.Count != 0))
                        {
                            await WaitingAction(actionHistoryCountBeforeWaiting);    
                        }
                        
                        var actionHistoryCountAfterWaiting = _actionHistory.Count;
                        if (actionHistoryCountBeforeWaiting == actionHistoryCountAfterWaiting)
                        {
                            var nextPlayer = GetNextPlayer();
                            _actionRequestsQueue.Enqueue(new ActionRequest
                            {
                                PlayerActionType = PlayerInRoomAction.FetchTile,
                                Player = nextPlayer,
                            });
                        }
                    }
                    break;
                
                case PlayerInRoomAction.FetchTile:
                    player.IsThisPlayerTurn = true;
                    _currentTurnIndex = _turnIndexs.Find(player.Seat);
                    var firstTileOfRest = _allTiles[0];
                    _allTiles.RemoveAt(0);
                    player.GangChecAfterFetchTile(firstTileOfRest);
                    player.WinCheck(firstTileOfRest);
                    player.HandTiles.Add(firstTileOfRest);
                    player.SortHandTiles();
                    
                    break;
                
                case PlayerInRoomAction.Eat:
                    player.IsThisPlayerTurn = true;
                    _currentTurnIndex = _turnIndexs.Find(player.Seat);
                    _actionHistory.TryPeek(out lastRq);
                    lastTile = lastRq.Player.SentTiles.Pop();
                    player.Eat(rq.Tiles[0], lastTile, rq.Tiles[1]);
                    
                    break;
                
                case PlayerInRoomAction.Pong:
                    player.IsThisPlayerTurn = true;
                    _currentTurnIndex = _turnIndexs.Find(player.Seat);
                    _actionHistory.TryPeek(out lastRq);
                    lastTile = lastRq.Player.SentTiles.Pop();
                    player.Pong(lastTile);
                    
                    break;
                
                case PlayerInRoomAction.Gang:
                    player.IsThisPlayerTurn = true;
                    _currentTurnIndex = _turnIndexs.Find(player.Seat);
                    _actionHistory.TryPeek(out lastRq);
                    lastTile = lastRq.Player.SentTiles.Pop();
                    player.Gang(lastTile);
                    if (lastRq.Player.GetPlayerId() == player.GetPlayerId() && lastRq.PlayerActionType == PlayerInRoomAction.FetchTile)
                    {
                     _actionRequestsQueue.Enqueue(new ActionRequest
                     {
                         PlayerActionType = PlayerInRoomAction.FetchTile,
                         Player = player
                     });   
                    }

                    break;
                
                case PlayerInRoomAction.AskToWin:
                    _actionHistory.TryPeek(out lastRq);
                    if (lastRq.PlayerActionType == PlayerInRoomAction.SendTile)
                    {
                        player.HandTiles = player.HandTiles.Append(lastRq.Player.SentTiles.Pop()).ToList();
                    }
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (rq.PlayerActionType!= PlayerInRoomAction.AskToWin)
            {
                await BroadcastRoomInfo();    
            }
            else
            {
                await WinThisRound(player);
            }
            
            _actionHistory.Push(rq);
        }
        
    }

    private void ClearAllPlayerAvailableAction()
    {
        foreach (var player in _players)
        {
            player.ClearAvailibleAction();
        }
        
    }

    private async Task WaitingAction(int actionHistoryCountBeforeWaiting)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));
        var seconds = 5;
        var waitingActionNotification = new WaitingActionNotification();
        
        while (await timer.WaitForNextTickAsync())
        {
            try
            {
                waitingActionNotification.LeftSeconds = seconds;
                var timerSeconds = JsonConvert.SerializeObject(waitingActionNotification);
                await BroadcastToRoomPlayers(timerSeconds);
                seconds--;
                if (seconds<0 || _actionHistory.Count > actionHistoryCountBeforeWaiting)
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
        var nextPlayer = GetNextPlayer();
        
        player.SentTiles.TryPeek(out var lastTile);
        nextPlayer.EatCheck(lastTile);
        
        foreach (var otherPlayer in _players.Where(p=>p.GetPlayerId()!=player.GetPlayerId()))
        {
            otherPlayer.PongCheck(lastTile); 
            otherPlayer.WinCheck(lastTile); 
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