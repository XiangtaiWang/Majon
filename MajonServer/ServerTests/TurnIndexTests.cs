using Server;
using Server.Classes;
using Xunit.Abstractions;

namespace ServerTests;

public class TurnIndexTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TurnIndexTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void TurnIndexShouldBeLoop()
    {
        var turnIndex = new TurnIndex();
        var linkedListNode = turnIndex.Find(SeatDirection.North);
        linkedListNode = turnIndex.GoNext(linkedListNode.Value);
        Assert.Equal(SeatDirection.East, linkedListNode.Value);

    }
}