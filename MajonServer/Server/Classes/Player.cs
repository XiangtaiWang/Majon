using System.Net.WebSockets;

namespace Server;

public class Player : IPlayer
{
    private int _playerId;
    public int PlayerId => _playerId;
    public SeatDirection Seat;
    public List<Tile> HandTiles;
    public Stack<Tile> SentTiles;
    public List<Tile> EatOrPongTiles;
    public List<List<Tile>> EatOptions;
    public List<PlayerAvailableActionInGame> AvailableActions;
    public bool IsThisPlayerTurn = false;
    private WebSocket _connection;
    private GameRoom _gameRoom;

    public Player(int playerId, WebSocket connection)
    {
        _playerId = playerId;
        _connection = connection;
        SentTiles = new Stack<Tile>();
        AvailableActions = new List<PlayerAvailableActionInGame>();
        EatOrPongTiles = new List<Tile>();
        EatOptions = new List<List<Tile>>();
    }

    public int GetPlayerId()
    {
        return _playerId;
    }

    public void Win()
    {
        throw new NotImplementedException();
    }

    public void SetHandTiles(List<Tile> tiles)
    {
        HandTiles = tiles;
    }

    public List<Tile> GetTilesInHand()
    {
        return HandTiles;
    }

    public void SendTile(Tile tile)
    {
        throw new NotImplementedException();
    }

    public void FetchTile()
    {
        throw new NotImplementedException();
    }

    public void Pong(Tile lastTile)
    {
        HandTiles.Remove(lastTile);
        HandTiles.Remove(lastTile);
        EatOrPongTiles.AddRange(new List<Tile>()
        {
            lastTile,
            lastTile,
            lastTile
        });
    }

    public GameRoom GetCurrentRoom()
    {
        return _gameRoom;
    }

    public void SetRoom(GameRoom gameRoom)
    {
        _gameRoom = gameRoom;
    }

    public WebSocket GetWebSocket()
    {
        return _connection;
    }

    public void PongCheck(Tile tile)
    {
        var count = HandTiles.FindAll(t => t.Equals(tile)).Count;
        // var count = HandTiles.Where(t => t == tile).Count();
        if (count >= 2)
        {
            AvailableActions.Add(PlayerAvailableActionInGame.Pong);
        }
    }

    public void EatCheck(Tile tile)
    {
        if (tile.CanOnlyPongTileType.Contains(tile.TileType))
        {
            return;
        }

        var tileMinos1 = new Tile(tileType: tile.TileType, tileNumber: tile.TileNumber-1);
        var tileMinos2 = new Tile(tileType: tile.TileType, tileNumber: tile.TileNumber-2);
        var tilePlus1 = new Tile(tileType: tile.TileType, tileNumber: tile.TileNumber+1);
        var tilePlus2 = new Tile(tileType: tile.TileType, tileNumber: tile.TileNumber+2);
        
        var handTiles = HandTiles.Distinct().ToList();
        
        if (handTiles.Contains(tileMinos2) && handTiles.Contains(tileMinos1))
        {
            EatOptions.Add([tileMinos2, tileMinos1]);
        }
        if ((handTiles.Contains(tileMinos1) && handTiles.Contains(tilePlus1)))
        {
            EatOptions.Add([tileMinos1, tilePlus1]);
        }
        if(handTiles.Contains(tilePlus1) && handTiles.Contains(tilePlus2))
        {
            EatOptions.Add([tilePlus1, tilePlus2]);
        }

        if (EatOptions.Count != 0)
        {
            AvailableActions.Add(PlayerAvailableActionInGame.Eat);
        }
    }

    public void ClearAvailibleAction()
    {
        AvailableActions.Clear();
        EatOptions.Clear();
    }

    public void SortHandTiles()
    {
        HandTiles = HandTiles.OrderBy(t => t.TileType).ThenBy(t => t.TileNumber).ToList();
    }

    public void WinCheck(Tile newTile)
    {
        HandTiles.Add(newTile);
        if (HandTiles.Count % 3 != 2)
            return;
        for (int i = 0; i < HandTiles.Count; i++)
        {
            Tile pairTile = HandTiles[i];
            List<Tile> remainingTiles = new List<Tile>(HandTiles);
            
            if (remainingTiles.Count(t => t.Equals(pairTile)) >= 2)
            {
                // 移除該對子
                remainingTiles.Remove(pairTile);
                remainingTiles.Remove(pairTile);
                
                if (CanFormMelds(remainingTiles))
                {
                    AvailableActions.Add(PlayerAvailableActionInGame.Win);
                }
            }
        }

        HandTiles.Remove(newTile);
    }
    private bool CanFormMelds(List<Tile> tiles)
    {
        if (tiles.Count == 0)
            return true; // 全部拆完，表示可以胡牌

        Tile firstTile = tiles[0];
        List<Tile> remainingTiles = new List<Tile>(tiles);

        // 嘗試組成刻子（3張相同的牌）
        if (remainingTiles.Count(t => t.Equals(firstTile)) >= 3)
        {
            remainingTiles.Remove(firstTile);
            remainingTiles.Remove(firstTile);
            remainingTiles.Remove(firstTile);
            if (CanFormMelds(remainingTiles))
            {
                return true;
            }
        }

        // 嘗試組成順子（僅適用於數字牌：萬、條、筒）
        if (firstTile.TileType == TileType.One || firstTile.TileType == TileType.Tiao || firstTile.TileType == TileType.Tong)
        {
            Tile secondTile = new Tile(firstTile.TileType, firstTile.TileNumber + 1);
            Tile thirdTile = new Tile(firstTile.TileType, firstTile.TileNumber + 2);

            if (remainingTiles.Exists(t => t.Equals(secondTile)) && remainingTiles.Exists(t => t.Equals(thirdTile)))
            {
                remainingTiles.Remove(firstTile);
                remainingTiles.Remove(secondTile);
                remainingTiles.Remove(thirdTile);
                if (CanFormMelds(remainingTiles))
                {
                    return true;
                }
            }
        }

        return false; // 無法組成順子或刻子，返回 false
    }

    public void Gang(Tile lastTile)
    {
        if (AlreadyPong(lastTile))
        {
            EatOrPongTiles.Insert(EatOrPongTiles.IndexOf(lastTile), lastTile);
        }
        else
        {
            HandTiles.Remove(lastTile);
            HandTiles.Remove(lastTile);
            HandTiles.Remove(lastTile);
            EatOrPongTiles.AddRange(new List<Tile>()
            {
                lastTile,
                lastTile,
                lastTile,
                lastTile
            });
        }
    }

    private bool AlreadyPong(Tile lastTile)
    {
        return EatOrPongTiles.Count(t => t.Equals(lastTile))==3;
    }

    public void GangChecAfterFetchTile(Tile newTile)
    {
        if (HandTiles.Count(t => t.Equals(newTile))==3 || SentTiles.Count(t => t.Equals(newTile))==3)
        {
            AvailableActions.Add(PlayerAvailableActionInGame.Gang);
        }
    }

    public void Eat(Tile myHandTile1, Tile eatenTile, Tile myHandTile2)
    {
        HandTiles.Remove(myHandTile1);
        HandTiles.Remove(myHandTile2);
        EatOrPongTiles.AddRange([myHandTile1, eatenTile, myHandTile2]);
    }
}