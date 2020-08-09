using System;
using Pipeline.Exceptions;
using Pipeline.InnerContracts;

namespace Pipeline
{
    /// <summary>
    /// Pipeline item contract.
    /// </summary>
    public interface IPipelineItem : IPostActionable
    {
        /// <summary>
        /// Pipeline item id.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Executes item.
        /// </summary>
        void Execute();

        /// <summary>
        /// Executes pipeline item with args.
        /// </summary>
        /// <param name="args">Args</param>
        void Execute(object[] args);
    }
}