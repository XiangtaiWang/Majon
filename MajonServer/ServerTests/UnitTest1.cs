using Server;
using Server.Classes;
using Xunit.Abstractions;

namespace ServerTests;

public class UnitTest1
{
    private readonly ITestOutputHelper _testOutputHelper;

    public UnitTest1(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Test1()
    {
        var turnIndex = new TurnIndex();
        // var directions = turnIndex.Indexs;
        var linkedListNode = turnIndex.Find(SeatDirection.South);
        for (int i = 0; i < 10; i++)
        {
            _testOutputHelper.WriteLine(linkedListNode.Value.ToString());
            linkedListNode = turnIndex.GoNext(linkedListNode.Value);
        }
        // foreach (var seatDirection in directions)
        // {
        //     _testOutputHelper.WriteLine(seatDirection.ToString());
        // }

    }
}