namespace PathView.Tests;

public class PathEntriesTests
{
    [Test]
    public void Split_ReturnsEmptyArray_WhenNull()
    {
        Assert.That(PathEntries.Split(null), Is.Empty);
    }

    [Test]
    public void Split_RemovesEmptyAndWhitespaceEntries()
    {
        var result = PathEntries.Split(@"C:\Tools;;   ;C:\Windows\System32");

        Assert.That(result, Is.EqualTo(new[] { @"C:\Tools", @"C:\Windows\System32" }));
    }

    [Test]
    public void Split_TrimsWhitespaceAroundEntries()
    {
        var result = PathEntries.Split(@" C:\Tools ; C:\Windows\System32 ");

        Assert.That(result, Is.EqualTo(new[] { @"C:\Tools", @"C:\Windows\System32" }));
    }

    [Test]
    public void Split_PreservesOrderAndDuplicates()
    {
        var result = PathEntries.Split(@"C:\Tools;C:\Windows\System32;C:\Tools");

        Assert.That(result, Is.EqualTo(new[] { @"C:\Tools", @"C:\Windows\System32", @"C:\Tools" }));
    }
}
