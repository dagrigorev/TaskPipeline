using System;
using Pipeline.InnerContracts;

namespace Pipeline
{
    /// <summary>
    /// Base pipeline contract.
    /// </summary>
    public interface IPipelineBase : IRegistrable
    {
        /// <summary>
        /// Gets registered items count.
        /// </summary>
        int Count { get; }
        
        /// <summary>
        /// Executes pipeline with no args.
        /// </summary>
        void Execute();
        
        /// <summary>
        /// Executes pipeline with args.
        /// </summary>
        /// <param name="args"></param>
        void Execute(params object[] args);
    }
}