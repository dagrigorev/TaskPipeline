using System;
using Pipeline;
using Pipeline.Default;

namespace TestPipeline
{
    public static class PipelineFactory
    {
        /// <summary>
        /// Creates new execution expression.
        /// </summary>
        /// <returns></returns>
        public static IPipelineItemExecutionExpression CreateNewExpression()
        {
            return new PipeItemExecutionPredicate(() => true);
        }

        /// <summary>
        /// Creates new execution expression.
        /// </summary>
        /// <returns></returns>
        public static IPipelineItemExecutionExpression CreateNewExpression(Func<object[],bool> argsPredicate)
        {
            return new PipeItemExecutionPredicate(argsPredicate);
        }
        
        /// <summary>
        /// Creates new pipeline.
        /// </summary>
        /// <returns></returns>
        public static IPipelineBase CreateNewPipeline()
        {
            return new SequentialPipeline();
        }

        /// <summary>
        /// Creates new executable item.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IPipelineItem CreateNewPipeItem()
        {
            return new PiplineEmptyItem();
        }

        /// <summary>
        /// Creates new item that always raises exception.
        /// </summary>
        /// <returns></returns>
        public static IPipelineItem CreateErrorItem()
        {
            return new PipelineDelegateItem(() => throw new Exception("Test exception"));
        }
        
        /// <summary>
        /// Creates new item that executes action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IPipelineItem CreateDelegateItem(Action action)
        {
            return new PipelineDelegateItem(action);
        }
        
        /// <summary>
        /// Creates new item that executes action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IPipelineItem CreateDelegateItem(Action<object[]> action)
        {
            return new PipelineDelegateItem(action);
        }
    }
}