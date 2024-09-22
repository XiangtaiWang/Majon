using Server;
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
        var direction = turnIndex.Indexs;
        foreach (var seatDirection in direction)
        {
            _testOutputHelper.WriteLine(seatDirection.ToString());
        }
        // for (int i = 0; i < 20; i++)
        // {
        //     _testOutputHelper.WriteLine(direction.Value.ToString());
        //     direction = direction.Next;
        // }
    }
}