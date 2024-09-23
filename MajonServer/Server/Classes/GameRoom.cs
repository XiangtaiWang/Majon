using System.Collections.Concurrent;

namespace Server;

public class GameRoom : IGameRoom
{
    private List<Player> _players;
    private List<Tile> _allTiles;
    private List<Tile> _sentTiles;
    public int PlayerCount => _players.Count;
    private int _roomId;
    private bool _isGameRunning = false;
    private ConcurrentDictionary<SeatDirection, Player> _seatInfo = new();
    private short _initialTurnIndex = 1;
    private short _turnIndex = 1;
    
        
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

    public GameRoom(int roomId)
    {
        _roomId = roomId;
        _players = new List<Player>();
    }

    private void AllocateTiles(Random random)
    {
        _allTiles = _allTiles.OrderBy(x => random.Next()).ToList();
        foreach (var player in _players)
        {
            player.SetTiles(_allTiles[..16]);
            player._handTiles.Order();
            _allTiles.RemoveRange(0, 16);
            
        }
    }

    private void CreateTiles()
    {
        _sentTiles = new List<Tile>();
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
                _allTiles.Add(new Tile(tileType: letterTile));
            }
        }
    }
    public void StartGame()
    {
        CreateTiles();
        var random = new Random();
        AllocateTiles(random);
        AllocateSeats(random);
        _turnIndex = _initialTurnIndex;
        RoundStart();
    }

    private void RoundStart()
    {
        _seatInfo.TryGetValue((SeatDirection)_turnIndex, out var thisTurnPlayer);
        thisTurnPlayer.IsThisPlayerTurn = true;

        Broadcast();
    }

    private void Broadcast()
    {
        foreach (var player in _players)
        {
            // player.Display()
        }
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
            _seatInfo.TryAdd(direction, _players[seatDirections.IndexOf(direction)]);
        }
        
        throw new NotImplementedException();
    }

    public void WinThisRound()
    {
        //TODO

    }
    

    public void GameFinish()
    {
        throw new NotImplementedException();
    }
    

    public void PlayerJoin(Player player)
    {
        _players.Add(player);
        
        if (_players.Count==4)
        {
            StartGame();
        }
    }

    public void PlayerLeave(IPlayer player)
    {
        
        throw new NotImplementedException();
    }

    public List<Player> IsPlayerIdInThisRoom()
    {
        return _players;
    }

    public void HandleRoomMessage(Player player, string[] messageParts)
    {
        var actionType = (PlayerInRoomAction)int.Parse(messageParts[0]);
        if (player.IsThisPlayerTurn)
        {
            switch (actionType)
            {
                case PlayerInRoomAction.SendTile:
                    var tileType = (TileType) short.Parse(messageParts[1]);
                    var tileNumber = short.Parse(messageParts[2]);
                    var tile = player._handTiles.First(tile =>
                        tile._tileType == tileType && tile._tileNumber == tileNumber);
                    player._handTiles.Remove(tile);

                    PlayerSentTile(tile);
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

    private void PlayerSentTile(Tile tile)
    {
        _sentTiles.Add(tile);
        UpdatePlayersStatus();
    }

    private void UpdatePlayersStatus()
    {
        _seatInfo.TryGetValue((SeatDirection)_turnIndex, out var player);
        player.IsThisPlayerTurn = false;
        _turnIndex++;
        if (_turnIndex>=5)
        {
            _turnIndex = 1;    
        }
        
        _seatInfo.TryGetValue((SeatDirection)_turnIndex, out var nextPlayer);
        nextPlayer.UpdateAvailableActions(_sentTiles.Last());
        
        
        Broadcast();
    }
    
}