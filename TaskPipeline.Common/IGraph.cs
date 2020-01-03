using System;
using System.Collections.Generic;
using System.Text;

namespace TaskPipeline.Common
{
    /// <summary>
    /// Graph definition
    /// </summary>
    /// <typeparam name="TNode">Node type</typeparam>
    /// <typeparam name="TEdge">Edge type</typeparam>
    interface IGraph<TValue, TNode, TEdge> where TNode : IEquatable<TNode>
    {
        int EdgesCount { get; }
        int NodesCount { get; }

        void AddNode(TNode node);
        void RemoveNode(TNode node);
        void RemoveNode<TId>(TId nodeId);
        TNode GetNodeById<TId>(TId nodeId);
        void SetEdge(TNode srcNode, TNode dstNode);
        void SetEdge<TId>(TId srcNodeId, TId dstNodeId);
        void UpdateNodes(Action<TNode> updater);
        bool AreNodesConnected<TId>(TId srcId, TId dstId);
    }
}
