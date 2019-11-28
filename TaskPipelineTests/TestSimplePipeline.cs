using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    using Pipeline.Runner;

    public class TestsSimplePipeline
    {
        SimplePipeline _pipeline;

        [SetUp]
        public void Setup()
        {
            _pipeline = new SimplePipeline();
        }

        [Test]
        public void TestExecute()
        {
            try
            {
                _pipeline.Execute(RandomWaitMethod);
            }
            catch
            {
                Assert.Fail();
            }
        }

        public void RandomWaitMethod()
        {
            var random = new Random();
            Thread.Sleep(random.Next(3000, 10000));
        }
    }
}