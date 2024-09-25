using System.Net.WebSockets;
using Server;
using Server.Classes;
using Xunit.Abstractions;

namespace ServerTests;

public class PlayerTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private Player _player;
    private int _playerId;

    public PlayerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _playerId = 999;
        _player = new Player(_playerId, new ClientWebSocket());
    }
    
    
    [Fact]
    public void TestPongCheck()
    {
        var tiles = new List<Tile>()
        {
            new Tile(TileType.EastWind, 0),
            new Tile(TileType.EastWind, 0)
        };
        _player.SetHandTiles(tiles);
        _player.PongCheck(new Tile(TileType.EastWind, 0));
        Assert.Contains(PlayerAvailableActionInGame.Pong, _player.AvailableActions);
    }
    
    [Fact]
    public void TestEatCheck()
    {
        var tiles = new List<Tile>()
        {
            new Tile(TileType.One, 1),
            new Tile(TileType.One, 2)
        };
        _player.SetHandTiles(tiles);
        _player.EatCheck(new Tile(TileType.One, 3));
        Assert.Contains(PlayerAvailableActionInGame.Eat, _player.AvailableActions);
    }
    [Fact]
    public void TestEatCheck_Middle()
    {
        var tiles = new List<Tile>()
        {
            new Tile(TileType.One, 1),
            new Tile(TileType.One, 3)
        };
        _player.SetHandTiles(tiles);
        _player.EatCheck(new Tile(TileType.One, 2));
        Assert.Contains(PlayerAvailableActionInGame.Eat, _player.AvailableActions);
    }
}