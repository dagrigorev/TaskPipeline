using System;
using System.Collections.Generic;
using System.Linq;
using Pipeline.Exceptions;
using Pipeline.InnerContracts;

namespace Pipeline.Default
{
    /// <summary>
    /// Sequential pipeline implementation.
    /// </summary>
    public class SequentialPipeline : IPipelineBase, IPostActionable
    {
        private Dictionary<IPipelineItemExecutionExpression, IPipelineItem> _items;
        private Action _continueAction;
        private Action _successAction;
        private Action<PipelineItemExecutionException> _errorAction;
        
        public int Count => _items.Count;
        
        public SequentialPipeline()
        {
            _items = new Dictionary<IPipelineItemExecutionExpression, IPipelineItem>();    
        }
        
        public void Register(IPipelineItemExecutionExpression expression, IPipelineItem item)
        {
            if(expression == null || item == null)
                throw new ArgumentNullException();
            
            if (!_items.ContainsKey(expression))
                _items.Add(expression, item);
            else
                _items[expression] = item;
        }

        /// <summary>
        /// Unregisters pipeline item. 
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="ArgumentNullException">When item is null.</exception>
        /// <exception cref="InvalidOperationException">When item not found in pipeline.</exception>
        public void UnRegister(IPipelineItem item)
        {
            if(item == null)
                throw  new ArgumentNullException();
            
            var (key, _) = _items.FirstOrDefault(kv => kv.Value.Equals(item));
            if (key != null)
                _items.Remove(key);
            else
                throw new InvalidOperationException();
        }

        public void Execute()
        {
            foreach (var key in _items.Keys)
            {
                Execute(key, _items[key]);
            }
        }

        public void Execute(params object[] args)
        {
            foreach (var key in _items.Keys)
            {
                Execute(key, _items[key], args);
            }
        }

        private void Execute(IPipelineItemExecutionExpression expr, IPipelineItem item)
        {
            if (expr != null && item != null && expr.CanExecute())
            {
                item.Execute();
            }
        }
        
        /// <summary>
        /// Executes single item.
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="item"></param>
        /// <param name="args"></param>
        private void Execute(IPipelineItemExecutionExpression expr, IPipelineItem item, object[] args)
        {
            try
            {
                if (expr != null && item != null && expr.CanExecute(args))
                {
                    // TODO: Pass args here
                    item.Execute(args);
                    
                    _successAction?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _errorAction?.Invoke(new PipelineItemExecutionException(ex));
            }
            
            _continueAction?.Invoke();
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
    }
}