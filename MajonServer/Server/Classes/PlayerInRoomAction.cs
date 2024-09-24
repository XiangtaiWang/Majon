namespace Server.Classes;

public enum PlayerInRoomAction
{
    Unknown = 0,
    SendTile = 1,
    FetchTile = 2,
    AskToWin = 3,
    LeaveRoom = 3
}