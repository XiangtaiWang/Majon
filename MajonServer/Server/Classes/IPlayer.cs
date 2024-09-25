using System.Net.Sockets;
using System.Net.WebSockets;

namespace Server;

public interface IPlayer
{
    public int GetPlayerId();
    public void Win();
    public void SetHandTiles(List<Tile> tiles);
    public List<Tile> GetTilesInHand();
    public void SendTile(Tile tile);
    public void FetchTile();
    public void Pong(Tile tile);
    public void Eat(Tile myHandTile1, Tile eatenTile, Tile myHandTile2);
    public GameRoom GetCurrentRoom();
    void SetRoom(GameRoom gameRoom);
    public WebSocket GetWebSocket();
    public void PongCheck(Tile tile);
}