namespace Server.Classes;

public enum PlayerInRoomAction
{
    Unknown = 0,
    SendTile = 1,
    FetchTile = 2,
    AskToWin = 3,
    Eat = 4,
    Pong = 5,
    Gang = 6
    // LeaveRoom = 3
}