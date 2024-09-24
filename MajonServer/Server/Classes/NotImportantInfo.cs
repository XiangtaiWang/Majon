namespace Server;

public class NotImportantInfo(string message)
{
    public readonly MessageType MessageType = MessageType.CanIgnore;
    public string Message = message;
}