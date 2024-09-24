namespace Server;

public interface IGameRoom
{
    public Task StartGame();

    public void WinThisRound();

    public void GameFinish();
    public Task PlayerJoin(Player player);
    public void PlayerLeave(Player player);
    public List<Player> IsPlayerIdInThisRoom();
    void HandleRoomMessage(Player player, string[] messageParts);
    public void NewRound();
    public int GetRoomId();
    List<int> GetPlayers();
}