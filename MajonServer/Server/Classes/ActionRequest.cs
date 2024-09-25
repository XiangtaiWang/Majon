using Server.Classes;

namespace Server;

public class ActionRequest
{
    public PlayerInRoomAction PlayerActionType;
    public Player Player;
    public List<Tile> Tiles;
}