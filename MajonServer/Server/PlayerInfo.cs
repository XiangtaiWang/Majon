namespace Server;

public class PlayerInfo
{
    public MessageType MessageType = MessageType.PlayerInfo;
    public int PlayerId { get; set; }
    
    public PlayerInfo(int getPlayerId)
    {
        PlayerId = getPlayerId;
    }
    
}