using System;
using Pipeline.Exceptions;

namespace Pipeline.InnerContracts
{
    /// <summary>
    /// Pipeline item post actions contract.
    /// </summary>
    public interface IPostActionable
    {
        /// <summary>
        /// Continues item executing in every time.
        /// </summary>
        void ContinueWith(Action action);
        
        /// <summary>
        /// Do some action when item executed successfully.
        /// </summary>
        /// <param name="action"></param>
        void WhenSuccess(Action action);

        /// <summary>
        /// Dome some action when error.
        /// </summary>
        /// <param name="action"></param>
        void WhenError(Action<PipelineItemExecutionException> action);
    }
}