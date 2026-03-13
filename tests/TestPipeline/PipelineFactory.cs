using Pipeline;
using Pipeline.Default;

namespace TestPipeline;

internal static class PipelineFactory
{
    public static SequentialPipeline CreateLegacyPipeline() => new();

    public static IPipelineItem CreateLegacyItem() => new PiplineEmptyItem();

    public static IPipelineItem CreateDelegateItem(Action<object[]> action) => new PipelineDelegateItem(action);

    public static IPipelineItem CreateErrorItem() => new PipelineDelegateItem(_ => throw new InvalidOperationException("boom"));

    public static IPipelineItemExecutionExpression CreateExpression(Func<object[], bool>? predicate = null)
        => predicate is null ? new PipeItemExecutionPredicate(() => true) : new PipeItemExecutionPredicate(predicate);

    public static object CreateNewPipeline() => CreateLegacyPipeline();

    public static object CreateNewPipeItem() => CreateLegacyItem();

    public static object CreateNewExpression() => CreateExpression();
}
