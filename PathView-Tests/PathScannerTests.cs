namespace PathView.Tests;

public class PathScannerTests
{
    private static readonly string ExistingDir = Path.GetTempPath().TrimEnd('\\');
    private static readonly string MissingDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    [Test]
    public void Scan_OrdersMachineBeforeUser_WithContinuousIndex()
    {
        var result = PathScanner.Scan($@"{ExistingDir};C:\Windows", $@"C:\Tools");

        Assert.That(result.Select(e => (e.Index, e.Origin)), Is.EqualTo(new[]
        {
            (1, PathOrigin.Machine),
            (2, PathOrigin.Machine),
            (3, PathOrigin.User),
        }));
    }

    [Test]
    public void Scan_FlagsMissing_WhenDirectoryDoesNotExist()
    {
        var result = PathScanner.Scan(MissingDir, null);

        Assert.That(result.Single().IsMissing, Is.True);
    }

    [Test]
    public void Scan_DoesNotFlagMissing_WhenDirectoryExists()
    {
        var result = PathScanner.Scan(ExistingDir, null);

        Assert.That(result.Single().IsMissing, Is.False);
    }

    [Test]
    public void Scan_FlagsDuplicate_AcrossMachineAndUserSections()
    {
        var result = PathScanner.Scan(ExistingDir, ExistingDir);

        Assert.That(result[0].DuplicateOfIndexes, Is.Empty);
        Assert.That(result[1].DuplicateOfIndexes, Is.EqualTo(new[] { 1 }));
    }

    [Test]
    public void Scan_AccumulatesAllPriorIndexes_ForRepeatedDuplicates()
    {
        var result = PathScanner.Scan($@"{ExistingDir};{ExistingDir};{ExistingDir}", null);

        Assert.That(result[0].DuplicateOfIndexes, Is.Empty);
        Assert.That(result[1].DuplicateOfIndexes, Is.EqualTo(new[] { 1 }));
        Assert.That(result[2].DuplicateOfIndexes, Is.EqualTo(new[] { 1, 2 }));
    }

    [Test]
    public void Scan_TreatsTrailingSlashAndCase_AsDuplicates()
    {
        var result = PathScanner.Scan($@"{ExistingDir.ToUpperInvariant()}\;{ExistingDir.ToLowerInvariant()}", null);

        Assert.That(result[1].DuplicateOfIndexes, Is.EqualTo(new[] { 1 }));
    }
}
