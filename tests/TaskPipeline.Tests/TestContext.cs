namespace TaskPipeline.Tests;

using System.Collections.Concurrent;

internal sealed class TestContext
{
    public ConcurrentQueue<string> Events { get; } = new();

    public int Counter { get; set; }

    public bool Condition { get; set; }

    public bool SkipSecondBranch { get; set; }

    public bool FailPrimary { get; set; }

    public int BranchSum { get; set; }
}
