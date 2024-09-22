using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

namespace Server;

public class Player : IPlayer
{
    private int _playerId;
    public List<Tile> _handTiles;
    public List<Tile> _eatOrPongTiles;
    private WebSocket _connection;
    private IGameRoom _gameRoom;
    public List<PlayerAvailableActionInGame> AvailableActions;
    public bool IsThisPlayerTurn { get; set; }

    public Player(int playerId, WebSocket connection)
    {
        _playerId = playerId;
        _connection = connection;
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
        _handTiles = tiles;
    }

    public List<Tile> GetTilesInHand()
    {
        return _handTiles;
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

    // public IGameRoom AskCreateRoom()
    // {
    //     //TODO
    //     var room = GameServer.CreateRoom(this);
    //     return room;
    // }

    public async Task Display(string information)
    {
        //todo: need add other info on the board
        // var message = GetTilesInHand().ToString();
        var data = "hellow world";
        var message = Encoding.ASCII.GetBytes(data);  


        await _connection.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, CancellationToken.None);

        // await _webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(information)), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public IGameRoom GetCurrentRoom()
    {
        return _gameRoom;
    }

    public void SetRoom(IGameRoom gameRoom)
    {
        _gameRoom = gameRoom;
    }

    public WebSocket GetWebSocket()
    {
        return _connection;
    }

    public void UpdateAvailableActions(Tile tile)
    {
        
        throw new NotImplementedException();
    }
}