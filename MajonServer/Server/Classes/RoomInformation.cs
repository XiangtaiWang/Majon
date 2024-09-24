namespace Server;

public class RoomInformation()
{
    public readonly MessageType MessageType = MessageType.Room;
    public int RoomId;
    public List<int> Players;
    public List<Player> PlayersInfo;
    // public int WhosePlayerTurn => PlayersInfo.Find(p => p.IsThisPlayerTurn).GetPlayerId();
    public Tile LastSentTile;

}