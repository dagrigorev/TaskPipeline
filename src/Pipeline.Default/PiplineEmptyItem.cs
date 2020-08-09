using System;
using Pipeline.Exceptions;

namespace Pipeline.Default
{
    /// <summary>
    /// Empty item implementation.
    /// </summary>
    public class PiplineEmptyItem : IPipelineItem
    {
        private Action _continueAction;
        private Action _successAction;
        
        public Guid Id { get; }

        public PiplineEmptyItem()
        {
            Id = Guid.NewGuid();    
        }
        
        public void ContinueWith(Action action)
        {
            _continueAction = action;
        }

        public void WhenSuccess(Action action)
        {
            _successAction = action;
        }

        public void WhenError(Action<PipelineItemExecutionException> action)
        {
        }
        
        public void Execute()
        {
            _successAction?.Invoke();
            _continueAction?.Invoke();
        }

        public void Execute(object[] args)
        {
            Execute();
        }
    }
}