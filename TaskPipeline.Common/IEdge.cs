using System;
using System.Collections.Generic;
using System.Text;

namespace TaskPipeline.Common
{
    /// <summary>
    /// Graph edge definition. Includes fields for getting correct orientation.
    /// </summary>
    /// <typeparam name="TNode">Type os connected nodes values</typeparam>
    public interface IEdge<TNode> : IEquatable<IEdge<TNode>>
    {
        /// <summary>
        /// Generates when edge become empty
        /// </summary>
        event Action<IEdge<TNode>> OnEdgeBecomesEmpty;

        /// <summary>
        /// Source node 
        /// </summary>
        TNode SourceNode { get; set; }

        /// <summary>
        /// Destination node
        /// </summary>
        TNode DestinationNode { get; set; }
    }
}
