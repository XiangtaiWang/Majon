using System.Net.WebSockets;
using System.Text;

namespace Server;

public class Player : IPlayer
{
    private int _playerId;
    public int PlayerId => _playerId;
    public SeatDirection Seat;
    public List<Tile> HandTiles;
    public List<Tile> SentTiles;
    public List<Tile> EatOrPongTiles;
    public List<PlayerAvailableActionInGame> AvailableActions;
    public bool IsThisPlayerTurn = false;
    private WebSocket _connection;
    private GameRoom _gameRoom;

    public Player(int playerId, WebSocket connection)
    {
        _playerId = playerId;
        _connection = connection;
        SentTiles = new List<Tile>();
    }

    public int GetPlayerId()
    {
        return _playerId;
    }

    public void Win()
    {
        throw new NotImplementedException();
    }

    public void SetTiles(List<Tile> tiles)
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

    public void UpdateAvailableActions(Tile tile)
    {
        //todo
        throw new NotImplementedException();
    }
}