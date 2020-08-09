using System;

namespace Pipeline.Default
{
    /// <summary>
    /// Simple predicate implementation.
    /// </summary>
    public class PipeItemExecutionPredicate : IPipelineItemExecutionExpression
    {
        private Func<bool> _predicate;
        private Func<object[], bool> _argsPredicate;

        /// <summary>
        /// Initializes new predicate without args.
        /// </summary>
        /// <param name="predicate"></param>
        public PipeItemExecutionPredicate(Func<bool> predicate)
        {
            _predicate = predicate;
        }
        
        /// <summary>
        /// Initializes new predicate with args.
        /// </summary>
        /// <param name="predicate"></param>
        public PipeItemExecutionPredicate(Func<object[], bool> predicate)
        {
            _argsPredicate = predicate;
        }
        
        public bool CanExecute()
        {
            return _predicate.Invoke();
        }

        public bool CanExecute(params object[] args)
        {
            return _argsPredicate.Invoke(args);
        }
    }
}