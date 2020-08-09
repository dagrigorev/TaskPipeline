using System;
using NUnit.Framework;

namespace TestPipeline
{
    public class TestsItemExecutionExpression
    {
        [SetUp]
        public void SetUp()
        {}

        [Test]
        public void TestCreateNewExpression()
        {
            var newExpression = PipelineFactory.CreateNewExpression();
            Assert.NotNull(newExpression);
        }

        [Test]
        public void TestExpressionCanExecuteWithNotArgs()
        {
            var newExpression = PipelineFactory.CreateNewExpression();
            Assert.IsTrue(newExpression.CanExecute());
        }

        [Test]
        public void TestExpressionCanExecuteWithArgs()
        {
            var newExpression = PipelineFactory.CreateNewExpression(args => args.Length > 0);
            Assert.IsTrue(newExpression.CanExecute(1,2,3,4));
        }
    }
}