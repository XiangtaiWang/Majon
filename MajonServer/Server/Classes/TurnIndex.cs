namespace Server;

public class TurnIndex
{
    // todo: make circle
    public readonly LinkedList<SeatDirection> Indexs;

    public TurnIndex()
    {
        Indexs= new LinkedList<SeatDirection>();
        Indexs.AddLast(SeatDirection.East);
        Indexs.AddLast(SeatDirection.South);
        Indexs.AddLast(SeatDirection.West);
        Indexs.AddLast(SeatDirection.North);
    }
}