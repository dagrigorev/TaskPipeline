using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskPipeline.Common.Impl
{
    public class ConnectedGraph<TValue> : IGraph<TValue, INode<Guid, TValue>, IEdge<INode<Guid, TValue>>>
    {
        private HashSet<IEdge<INode<Guid, TValue>>> _edges;

        public int EdgesCount => _edges.Count;

        public int NodesCount => _edges.Count(e => e.DestinationNode != null || e.SourceNode != null);

        public ConnectedGraph()
        {
            _edges = new HashSet<IEdge<INode<Guid, TValue>>>();        
        }

        /// <summary>
        /// Adds new node to graph. If node argument is null, method will generate <see cref="NullReferenceException"/>.
        /// </summary>
        /// <param name="node">Addable node.</param>
        public void AddNode(INode<Guid, TValue> node)
        {
            if(node == null) throw new NullReferenceException("Addable node cannot be represented as null value");

            if (!_edges.Any(edge => edge.DestinationNode != null && edge.DestinationNode.Equals(node) ||
                                    edge.SourceNode != null && edge.SourceNode.Equals(node)))
            {
                var edge = new Edge<INode<Guid, TValue>>(node, null);
                edge.OnEdgeBecomesEmpty += OnEdgeBecomesEmpty;
                _edges.Add(edge);
            }
        }

        private void OnEdgeBecomesEmpty(IEdge<INode<Guid, TValue>> obj)
        {
            obj.OnEdgeBecomesEmpty -= OnEdgeBecomesEmpty;
            _edges.RemoveWhere(edge => edge.DestinationNode == null && edge.SourceNode == null);
        }

        public INode<Guid, TValue> GetNodeById<TId>(TId nodeId)
        {
            if (nodeId.Equals(default(TId))) throw new ArgumentException("Invalid node id");

            var foundEdge = _edges.FirstOrDefault(edge => edge.DestinationNode != null && edge.DestinationNode.Id.Equals(nodeId) ||
                                                 edge.SourceNode != null && edge.SourceNode.Id.Equals(nodeId));

            if (foundEdge == null) return null;

            if (foundEdge.DestinationNode != null && foundEdge.DestinationNode.Id.Equals(nodeId))
                return foundEdge.DestinationNode;
            else if (foundEdge.SourceNode != null && foundEdge.SourceNode.Id.Equals(nodeId))
                return foundEdge.SourceNode;

            return null;
        }

        public void SetEdge(INode<Guid, TValue> srcNode, INode<Guid, TValue> dstNode)
        {
            throw new NotImplementedException();
        }

        public void SetEdge<TId>(TId srcNodeId, TId dstNodeId)
        {
            if(srcNodeId.Equals(dstNodeId) || srcNodeId.Equals(default(TId)) || dstNodeId.Equals(default(TId)))
                throw new ArgumentException("Invalid id value");
            if(_edges.Count == 0) throw new InvalidOperationException();

            var srcEdge = _edges.FirstOrDefault(edge => edge.SourceNode != null && edge.SourceNode.Id.Equals(srcNodeId));
            var dstEdge = _edges.FirstOrDefault(edge => edge.SourceNode != null && edge.SourceNode.Id.Equals(dstNodeId) || edge.DestinationNode != null && edge.DestinationNode.Id.Equals(dstNodeId));

            if (srcEdge != null && dstEdge != null)
            {
                if (dstEdge.SourceNode.Id.Equals(dstNodeId))
                {
                    srcEdge.DestinationNode = dstEdge.SourceNode;
                    dstEdge.SourceNode = null;
                }
                else
                {
                    srcEdge.DestinationNode = dstEdge.DestinationNode;
                    dstEdge.DestinationNode = null;
                }
            }
        }

        public void RemoveNode(INode<Guid, TValue> node)
        {
            if (node != null && node.Id != Guid.Empty)
            {
                _edges.RemoveWhere(edge => edge.DestinationNode == node || edge.SourceNode == node);
            }
        }

        public void RemoveNode<TId>(TId nodeId)
        {
            throw new NotImplementedException();
        }

        public void UpdateNodes(Action<INode<Guid, TValue>> updater)
        {
            if (updater != null)
            {
                foreach(var edge in _edges)
                {
                    updater.Invoke(edge.SourceNode);
                    updater.Invoke(edge.DestinationNode);
                }
            }
        }

        public bool AreNodesConnected<TId>(TId srcId, TId dstId)
        {
            if(srcId.Equals(dstId)) throw new ArgumentException("Node id must be different");

            return _edges.Any(edge => edge.SourceNode != null && edge.DestinationNode != null && edge.SourceNode.Id.Equals(srcId) && edge.DestinationNode.Id.Equals(dstId));
        }
    }
}
