using System;
using NUnit.Framework;

namespace TestPipeline
{
    public class TestsPipelineItem
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void TestCreateNewItem()
        {
            var newItem = PipelineFactory.CreateNewPipeItem();
            Assert.NotNull(newItem);
        }

        [Test]
        public void TestItemExecuting()
        {
            var testCounter = 0;
            var item = PipelineFactory.CreateNewPipeItem();

            item.WhenSuccess(() => testCounter++);
            item.Execute();
            
            Assert.IsTrue(testCounter > 0);
        }

        [Test]
        public void TestItemContinueWith()
        {
            var testCounter = 0;
            var item = PipelineFactory.CreateNewPipeItem();

            item.ContinueWith(() => testCounter++);
            item.Execute();
            
            Assert.IsTrue(testCounter > 0);
        }

        [Test]
        public void TestItemWhenError()
        {
            var testCounter = 0;
            var item = PipelineFactory.CreateErrorItem();

            item.WhenError(ex => testCounter++);
            item.Execute();
            
            Assert.IsTrue(testCounter > 0);
        }
    }
}