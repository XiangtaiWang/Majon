using System.Collections;
using Server;

namespace ServerTests;

public class SuccessfulWinCheckTestDataGenerator : IEnumerable<object[]>
{
    private readonly List<object[]> _data =  new List<object[]>
    {
        new object[] { new List<Tile>()
        {
            new Tile(TileType.One, 1),
            new Tile(TileType.One, 1),
            new Tile(TileType.One, 1),
            
            new Tile(TileType.One, 2),
            
        }},
        new object[] { new List<Tile>()
        {
            new Tile(TileType.One, 3),
            new Tile(TileType.One, 3),
            new Tile(TileType.One, 3),
            
            new Tile(TileType.One, 2),
            
        }},
        new object[] { new List<Tile>()
        {
            new Tile(TileType.One, 1),
            new Tile(TileType.One, 1),
            new Tile(TileType.One, 1),
            
            new Tile(TileType.Tiao, 4),
            new Tile(TileType.Tiao, 5),
            new Tile(TileType.Tiao, 6),
            
            new Tile(TileType.Tong, 5),
            new Tile(TileType.Tong, 6),
            new Tile(TileType.Tong, 7),
            
            new Tile(TileType.One, 2),
            
        }},
    };

    public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    // IEnumerator IEnumerable.GetEnumerator()
    // {
    //     return GetEnumerator();
    // }
}