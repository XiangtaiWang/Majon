namespace Server;

public interface IGameRoom
{
    public Task StartGame();

    public Task WinThisRound(Player player);

    public void GameFinish();
    public Task PlayerJoin(Player player);
    public void PlayerLeave(Player player);
    public List<Player> IsPlayerIdInThisRoom();
    Task HandleRoomMessage(Player player, string[] messageParts);
    public void NewRound();
    public int GetRoomId();
    List<int> GetPlayers();
}