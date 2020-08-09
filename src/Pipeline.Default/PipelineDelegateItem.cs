using System;
using Pipeline.Exceptions;

namespace Pipeline.Default
{
    /// <summary>
    /// Delegate item implementation.
    /// </summary>
    public class PipelineDelegateItem : IPipelineItem
    {
        private Action _continueAction;
        private Action<PipelineItemExecutionException> _errorAction;
        private Action _successAction;
        private Action _delegateItem;
        private Action<object[]> _delegateArgsItem;
        private Exception _ex;
        
        public Guid Id { get; }

        private PipelineDelegateItem()
        {
            Id = Guid.NewGuid();    
        }

        /// <summary>
        /// Initializes new pipeline item.
        /// </summary>
        /// <param name="action"></param>
        /// <exception cref="ArgumentNullException">When action is null.</exception>
        public PipelineDelegateItem(Action action)
            :base()
        {
            if (action == null)
                throw new ArgumentNullException("Delegate cannot be null");
            
            _delegateItem = action;
        }
        
        /// <summary>
        /// Initializes new pipeline item.
        /// </summary>
        /// <param name="action"></param>
        /// <exception cref="ArgumentNullException">When action is null.</exception>
        public PipelineDelegateItem(Action<object[]> action)
            :base()
        {
            if (action == null)
                throw new ArgumentNullException("Delegate cannot be null");
            
            _delegateArgsItem = action;
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
            _errorAction = action;
        }

        public void Execute()
        {
            try
            {     
                _delegateItem?.Invoke();
                _successAction?.Invoke();
            }
            catch (Exception ex)
            {
                _errorAction?.Invoke(new PipelineItemExecutionException(ex));
            }
            
            _continueAction?.Invoke();
        }
        
        public void Execute(object[] args)
        {
            try
            {
                _delegateArgsItem.Invoke(args);
                _successAction?.Invoke();
            }
            catch (Exception ex)
            {
                _errorAction?.Invoke(new PipelineItemExecutionException(ex));
            }
            
            _continueAction?.Invoke();
        }
    }
}