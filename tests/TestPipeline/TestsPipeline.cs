using System;
using System.Linq;
using NUnit.Framework;
using Pipeline;

namespace TestPipeline
{
    /// <summary>
    /// Pipeline tests.
    /// </summary>
    public class TestsPipeline
    {
        [SetUp]
        public void Setup()
        {
            // TODO: Load implementations here.
        }

        [Test]
        public void TestSingleItemPipelineExecuting()
        {
            var testCounter = 0;
            var newPipeline = PipelineFactory.CreateNewPipeline();
            var newItem = PipelineFactory.CreateNewPipeItem();
            var newExpr = PipelineFactory.CreateNewExpression();
            
            newItem.WhenSuccess(() => testCounter++);
            newPipeline.Register(newExpr, newItem);
            newPipeline.Execute();
            
            Assert.IsTrue(testCounter > 0 && newExpr.CanExecute());
        }
        
        [Test]
        public void TestArgsItemPipelineExecuting()
        {
            var testCounter = 0;
            var newPipelineItem = PipelineFactory.CreateDelegateItem(args =>
            {
                if (args.Length > 0)
                    testCounter++;
            });
            var newPipeline = PipelineFactory.CreateNewPipeline();
            newPipeline.Register(PipelineFactory.CreateNewExpression(args => args.Length > 0), newPipelineItem);
            newPipeline.Execute(1, 2, 3, 4);
            
            Assert.IsTrue(testCounter > 0);
        }
        
        [Test]
        public void TestMultipleItemPipelineExecuting()
        {
            var testCounter = 0;
            var newPipeline = PipelineFactory.CreateNewPipeline();

            for (var i = 0; i < 100; i++)
            {
                var newItem = PipelineFactory.CreateNewPipeItem();
                var newExpr = PipelineFactory.CreateNewExpression();

                if(!newExpr.CanExecute()) 
                    throw new InvalidOperationException("Execution expression must be valid");
                
                newItem.WhenSuccess(() => testCounter++);
                newPipeline.Register(newExpr, newItem);
            }

            newPipeline.Execute();
            
            Assert.IsTrue(testCounter >= 99);
        }
        
        [Test]
        public void TestErrorItemPipelineExecuting()
        {
            var testCounter = 0;
            var newPipeline = PipelineFactory.CreateNewPipeline();
            var newItem = PipelineFactory.CreateErrorItem();
            var newExpr = PipelineFactory.CreateNewExpression();
            
            newItem.WhenError(ex => testCounter++);
            newPipeline.Register(newExpr, newItem);
            newPipeline.Execute();
            
            Assert.IsTrue(testCounter > 0 && newExpr.CanExecute());
        }

        [Test]
        public void TestPipelineItemRegister()
        {
            var newPipeline = PipelineFactory.CreateNewPipeline();
            newPipeline.Register(PipelineFactory.CreateNewExpression(), PipelineFactory.CreateNewPipeItem());
            Assert.IsTrue(newPipeline.Count > 0);
        }
        
        [Test]
        public void TestInvalidItemRegister()
        {
            var newPipeline = PipelineFactory.CreateNewPipeline();
            Assert.Throws<ArgumentNullException>(() =>
                newPipeline.Register(PipelineFactory.CreateNewExpression(), null));
        }

        [Test]
        public void TestInvalidExpressionRegister()
        {
            var newPipeline = PipelineFactory.CreateNewPipeline();
            Assert.Throws<ArgumentNullException>(() =>
                newPipeline.Register(null, PipelineFactory.CreateNewPipeItem()));
        }
        
        [Test]
        public void TestPipelineItemUnRegister()
        {
            var newPipeline = PipelineFactory.CreateNewPipeline();
            var newItem = PipelineFactory.CreateNewPipeItem();
            newPipeline.Register(PipelineFactory.CreateNewExpression(), newItem);
            newPipeline.UnRegister(newItem);
            Assert.IsTrue(newPipeline.Count == 0);
        }
        
        [Test]
        public void TestInvalidItemUnRegister()
        {
            var newPipeline = PipelineFactory.CreateNewPipeline();
            Assert.Throws<ArgumentNullException>(() =>
                newPipeline.UnRegister(null));
        }

        [Test]
        public void TestNonExistingItemUnRegister()
        {
            var newPipeline = PipelineFactory.CreateNewPipeline();
            var newItem = PipelineFactory.CreateNewPipeItem();
            
            newPipeline.Register(PipelineFactory.CreateNewExpression(), PipelineFactory.CreateNewPipeItem());
            
            Assert.Throws<InvalidOperationException>(() =>
                newPipeline.UnRegister(newItem));
        }
    }
}