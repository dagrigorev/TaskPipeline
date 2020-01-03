using System;

namespace TaskPipeline.Common
{
    /// <summary>
    /// Graph node definition
    /// </summary>
    /// <typeparam name="T">Type of node value</typeparam>
    public interface INode<TId, T> : IEquatable<INode<TId, T>>
    {
        /// <summary>
        /// Node ID
        /// </summary>
        TId Id { get; }

        /// <summary>
        /// Node value
        /// </summary>
        T Value { get; set; }
    }
}
