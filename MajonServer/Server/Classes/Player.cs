using System.Net.WebSockets;
using System.Text;

namespace Server;

public class Player : IPlayer
{
    private int _playerId;
    public int PlayerId => _playerId;
    public SeatDirection Seat;
    public List<Tile> HandTiles;
    public Stack<Tile> SentTiles;
    public List<Tile> EatOrPongTiles;
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

    public void Eat()
    {
        throw new NotImplementedException();
    }

    public void Pong()
    {
        throw new NotImplementedException();
    }

    public async Task Display(string information)
    {
        //todo: need add other info on the board
        // var message = GetTilesInHand().ToString();
        var data = "hellow world";
        var message = Encoding.ASCII.GetBytes(data);  


        await _connection.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, CancellationToken.None);

        // await _webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(information)), WebSocketMessageType.Text, true, CancellationToken.None);
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
        
        if ((handTiles.Contains(tileMinos2) && handTiles.Contains(tileMinos1)) ||
            (handTiles.Contains(tileMinos1) && handTiles.Contains(tilePlus1)) ||
            (handTiles.Contains(tilePlus1) && handTiles.Contains(tilePlus2)))
        {
            AvailableActions.Add(PlayerAvailableActionInGame.Eat);
        }
    }

    public void ClearAvailibleAction()
    {
        AvailableActions.Clear();
    }
}