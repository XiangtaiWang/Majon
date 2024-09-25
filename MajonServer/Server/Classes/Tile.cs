namespace Server;

public class Tile 
{
    public TileType TileType;
    public int TileNumber;
    public readonly List<TileType> CanOnlyPongTileType =
    [
        TileType.RedCenter,
        TileType.EarnMoney,
        TileType.WhiteBoard,
        TileType.EastWind,
        TileType.SouthWind,
        TileType.WestWind,
        TileType.NorthWind
    ];
    public readonly List<TileType> NeedNumberTileType =
    [
        TileType.One,
        TileType.Tiao,
        TileType.Tong
    ];

    public Tile(TileType tileType, int tileNumber)
    {
        // if (!_tileNeedNumber.Contains(tileType))
        // {
        //     throw new Exception("incorrect create tile");
        // }
        TileType = tileType;
        TileNumber = tileNumber;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        var other = (Tile)obj;
        return TileType == other.TileType && TileNumber == other.TileNumber;
        
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(TileType, TileNumber);
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