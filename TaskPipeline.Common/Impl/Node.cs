using System;
using System.Collections.Generic;
using System.Text;

namespace TaskPipeline.Common.Impl
{
    public class Node<T> : INode<Guid, T>
    {
        public Guid Id { get; }
        public T Value { get; set; }

        public Node() : this(Guid.NewGuid())
        {
        }

        public Node(Guid id)
        {
            this.Id = id;
            Value = default(T);
        }

        public bool Equals(INode<Guid, T> other)
        {
            return other != null && other.Id.Equals(Id);
        }
    }
}
