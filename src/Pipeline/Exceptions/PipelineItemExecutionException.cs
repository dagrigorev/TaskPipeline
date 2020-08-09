using System;

namespace Pipeline.Exceptions
{
    /// <summary>
    /// Pipeline item exception that raises when something wrong wrong in <see cref="IPipelineItem.Execute"/>.
    /// </summary>
    public class PipelineItemExecutionException : Exception
    {
        /// <summary>
        /// Pipeline item id.
        /// </summary>
        public Guid Id { get; set; }

        public PipelineItemExecutionException()
        {
        }

        public PipelineItemExecutionException(Exception ex)
            :base(ex.Message, ex.InnerException)
        {
            if (ex is PipelineItemExecutionException pex)
            {
                Id = pex.Id;
            }
        }
    }
}