namespace Server.Classes;

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

    public LinkedListNode<SeatDirection> Find(SeatDirection direction)
    {
        return Indexs.Find(direction);
    }

    public LinkedListNode<SeatDirection> GoNext(SeatDirection value)
    {
        var nextNode = Indexs.Find(value).Next;

        return nextNode==null?Indexs.First:nextNode;
    }
}