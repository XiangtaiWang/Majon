namespace Server;

public class LobbyInformation
{
    public readonly MessageType MessageType = MessageType.Lobby;
    public IEnumerable<int> Rooms;
}