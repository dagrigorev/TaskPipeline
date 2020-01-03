using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace TaskPipeline.Common.Impl
{
    public class Edge<TNode> : IEdge<TNode>
    {
        public event Action<IEdge<TNode>> OnEdgeBecomesEmpty;

        private TNode _srcNode;
        private TNode _dstNode;

        public TNode SourceNode
        {
            get => _srcNode;
            set
            {
                _srcNode = value;
                if (_srcNode == null && _dstNode == null)
                    OnEdgeBecomesEmpty?.Invoke(this);
            }
        }

        public TNode DestinationNode
        {
            get => _dstNode;
            set
            {
                _dstNode = value;
                if (_srcNode == null && _dstNode == null)
                    OnEdgeBecomesEmpty?.Invoke(this);
            }
        }

        public Edge()
        {
            SourceNode = default(TNode);
            DestinationNode = default(TNode);
        }

        public Edge(TNode src, TNode dst)
        {
            if(src.Equals(dst)) throw new ArgumentException();

            SourceNode = src;
            DestinationNode = dst;
        }

        public bool Equals(IEdge<TNode> other)
        {
            return other != null && (SourceNode == null && 
                                     DestinationNode == null && 
                                     other.SourceNode == null && 
                                     other.DestinationNode == null || 
                                     (SourceNode.Equals(other.SourceNode) && DestinationNode.Equals(other.DestinationNode)));
        }
    }
}
