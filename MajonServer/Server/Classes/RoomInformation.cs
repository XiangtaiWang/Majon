namespace Server;

public class RoomInformation()
{
    public readonly MessageType MessageType = MessageType.Room;
    public int RoomId;
    public List<int> Players;
    public List<Player> PlayersInfo;
    public List<Tile> SentTiles;
    public Tile LastSentTile;

}