namespace Server;

public class Tile
{
    public TileType _tileType;
    public short _tileNumber;
    private readonly List<TileType> _letterTiles =
    [
        TileType.RedCenter,
        TileType.EarnMoney,
        TileType.WhiteBoard,
        TileType.EastWind,
        TileType.SouthWind,
        TileType.WestWind,
        TileType.NorthWind
    ];
    private readonly List<TileType> _tileNeedNumber =
    [
        TileType.One,
        TileType.Tiao,
        TileType.Tong
    ];

    public Tile(TileType tileType, short tileNumber)
    {
        if (!_tileNeedNumber.Contains(tileType))
        {
            throw new Exception("incorrect create tile");
        }
        _tileType = tileType;
        _tileNumber = tileNumber;
    }
    public Tile(TileType tileType)
    {
        if (!_letterTiles.Contains(tileType))
        {
            throw new Exception("incorrect create tile");
        }

        _tileNumber = 0;
        _tileType = tileType;
    }
}

public enum TileType
{
    Unknown = 0,
    One = 1,
    Tiao = 2,
    Tong = 3,
    RedCenter = 4,
    EarnMoney = 5,
    WhiteBoard = 6,
    EastWind = 7,
    SouthWind = 8,
    WestWind = 9,
    NorthWind = 10
}