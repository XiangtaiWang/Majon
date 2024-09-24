namespace Server;

public class RoomInformation(string message)
{
    public readonly MessageType MessageType = MessageType.Room;
    public int RoomId;
    public List<int> Players;
    public string Message = message;
}