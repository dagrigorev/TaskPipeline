using System;
using Pipeline;

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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates new pipeline.
        /// </summary>
        /// <returns></returns>
        public static IPipelineBase CreateNewPipeline()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates new executable item.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IPipelineItem CreateNewPipeItem()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates new item that always raises exception.
        /// </summary>
        /// <returns></returns>
        public static IPipelineItem CreateErrorItem()
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Creates new item that executes action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IPipelineItem CreateDelegateItem(Action action)
        {
            throw new NotImplementedException();
        }
    }
}