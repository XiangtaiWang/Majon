namespace Server;

public interface IGameRoom
{
    public void StartGame();

    public void WinThisRound();

    public void GameFinish();
    public void PlayerJoin(Player player);
    public void PlayerLeave(IPlayer player);
    public List<Player> IsPlayerIdInThisRoom();
    void HandleRoomMessage(Player player, string[] messageParts);
    public void NewRound();
    public int GetRoomId();
}