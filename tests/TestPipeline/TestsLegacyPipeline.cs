using NUnit.Framework;
using Pipeline.Default;

namespace TestPipeline;

public class TestsLegacyPipeline
{
    [Test]
    public void Legacy_pipeline_executes_registered_item()
    {
        var counter = 0;
        var pipeline = PipelineFactory.CreateLegacyPipeline();
        var item = PipelineFactory.CreateLegacyItem();
        item.WhenSuccess(() => counter++);

        pipeline.Register(PipelineFactory.CreateExpression(), item);
        pipeline.Execute();

        Assert.That(counter, Is.EqualTo(1));
        Assert.That(pipeline.Count, Is.EqualTo(1));
    }

    [Test]
    public void Legacy_pipeline_passes_arguments()
    {
        var counter = 0;
        var pipeline = PipelineFactory.CreateLegacyPipeline();
        var item = PipelineFactory.CreateDelegateItem(args =>
        {
            if (args.Length == 2) counter++;
        });

        pipeline.Register(PipelineFactory.CreateExpression(args => args.Length == 2), item);
        pipeline.Execute("a", "b");

        Assert.That(counter, Is.EqualTo(1));
    }

    [Test]
    public void Legacy_pipeline_reports_errors_without_crashing_pipeline_wrapper()
    {
        var errors = 0;
        var pipeline = PipelineFactory.CreateLegacyPipeline();
        pipeline.WhenError(_ => errors++);
        pipeline.Register(PipelineFactory.CreateExpression(), PipelineFactory.CreateErrorItem());

        pipeline.Execute(42);

        Assert.That(errors, Is.EqualTo(1));
    }

    [Test]
    public void Legacy_pipeline_unregisters_item()
    {
        var pipeline = PipelineFactory.CreateLegacyPipeline();
        var item = PipelineFactory.CreateLegacyItem();
        pipeline.Register(PipelineFactory.CreateExpression(), item);

        pipeline.UnRegister(item);

        Assert.That(pipeline.Count, Is.EqualTo(0));
    }
}
