namespace Server;

internal class WaitingActionNotification
{
    public readonly MessageType MessageType = MessageType.WaitingAction;
    public int LeftSeconds;
}