using System.Net.Sockets;
using System.Net.WebSockets;

namespace Server;

public interface IPlayer
{
    public int GetPlayerId();
    public void Win();
    public void SetTiles(List<Tile> tiles);
    public List<Tile> GetTilesInHand();
    public void SendTile(Tile tile);
    public void FetchTile();
    public void Eat();
    public void Pong();
    // public IGameRoom AskCreateRoom();
    Task Display(string information);
    public IGameRoom GetCurrentRoom();
    void SetRoom(IGameRoom gameRoom);
    public WebSocket GetWebSocket();
    public void UpdateAvailableActions(Tile tile);
}