using System;
using NUnit.Framework;
using TaskPipeline.Common.Impl;

namespace TaskPipelineTests.Tests.Common
{
    [TestFixture]
    [Author("Dmitry Grigorev", "dmitryandreevichgr@gmail.com")]
    public class TestConnectedGraph
    {
        [Test]
        [Category("TestAdd")]
        public void TestAddNewNode()
        {
            var graph = new ConnectedGraph<int>();
            
            graph.AddNode(new Node<int>());
            
            Assert.AreEqual(graph.NodesCount, 1);
        }

        [Test]
        [Category("TestAdd")]
        public void TestAddNullValueAsNode()
        {
            var graph = new ConnectedGraph<int>();

            Assert.Throws(typeof(NullReferenceException), () => graph.AddNode(null));
        }

        [Test]
        [Category("TestAdd")]
        public void TestAddExistedNode()
        {
            var graph = new ConnectedGraph<int>();
            var id = Guid.NewGuid();

            graph.AddNode(new Node<int>(id));
            graph.AddNode(new Node<int>(id));

            Assert.AreEqual(graph.NodesCount, 1);
        }

        [Test]
        [Category("TestGetNodeById")]
        public void TestGetNodeByIdExistedNode()
        {
            var graph = new ConnectedGraph<int>();
            var nodeId = Guid.NewGuid();
            var nodeValue = 99;

            graph.AddNode(new Node<int>(nodeId)
            {
                Value = nodeValue
            });
            
            Assert.AreEqual(graph.GetNodeById(nodeId).Value, nodeValue);
        }

        [Test]
        [Category("TestGetNodeById")]
        public void TestGetNodeByIdNonExistedNode()
        {
            var graph = new ConnectedGraph<int>();

            Assert.IsNull(graph.GetNodeById(Guid.NewGuid()));
        }

        [Test]
        [Category("TestGetNodeById")]
        public void TestGetNodeByIdWithInvalidId()
        {
            var graph = new ConnectedGraph<int>();

            Assert.Throws(typeof(ArgumentException), () => graph.GetNodeById(Guid.Empty));
        }

        [Test]
        [Category("TestSetEdge")]
        public void TestSetEdgeWithValidArgs()
        {
            var graph = new ConnectedGraph<int>();
            var srcId = Guid.NewGuid();
            var dstId = Guid.NewGuid();

            graph.AddNode(new Node<int>(srcId));
            graph.AddNode(new Node<int>(dstId));
            graph.AddNode(new Node<int>(new Guid()));
            graph.AddNode(new Node<int>(new Guid()));
            graph.SetEdge(srcId, dstId);

            Assert.IsTrue(graph.AreNodesConnected(srcId, dstId));
        }

        [Test]
        [Category("TestSetEdge")]
        public void TestSetEdgeWithInvalidArgs()
        {
            var graph = new ConnectedGraph<int>();
            Assert.Throws(typeof(ArgumentException), () => graph.SetEdge(Guid.Empty, Guid.Empty));
        }

        [Test]
        [Category("TestSetEdge")]
        public void TestSetEdgeForEmptyGraph()
        {
            var graph = new ConnectedGraph<int>();
            Assert.Throws(typeof(InvalidOperationException), () => graph.SetEdge(Guid.NewGuid(), Guid.NewGuid()));
        }
    }
}