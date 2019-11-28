using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    using Pipeline.Queue;

    public class TestsTaskQueue
    {
        ITaskQueue _tasks;

        [SetUp]
        public void Setup()
        {
            _tasks = new TasksQueue();
        }

        [Test]
        public void TestAdd()
        {
            try
            {
                _tasks.PushTask(new Task(() => Print("Helllo")));
            }
            catch
            {
                Assert.Fail();
            }
        }

        [Test]
        public void TestRemove()
        {
            try
            {
                _tasks.PopTask();
            }
            catch
            {
                Assert.Fail();
            }
        }

        [Test]
        public void TestRun()
        {
            try
            {
                _tasks.PushTask(new Task(() => Print("Helllo")));
                _tasks.PushTask(new Task(() => Print("World")));
                _tasks.PushTask(new Task(() => Print("1234")));
                _tasks.PushTask(new Task(() => Print("abcde")));

                _tasks.RunAll();
            }
            catch
            {
                Assert.Fail();
            }
        }

        [Test]
        public void TestWait()
        {
            _tasks.PushTask(new Task(() => RandomWaitMethod()));
            _tasks.PushTask(new Task(() => RandomWaitMethod()));
            _tasks.PushTask(new Task(() => RandomWaitMethod()));
            _tasks.PushTask(new Task(() => RandomWaitMethod()));

            _tasks.RunAll();
            var count = _tasks.WaitAll();
            Assert.AreEqual(count, 4);
        }

        /// <summary>
        /// Simple method
        /// </summary>
        /// <param name="message"></param>
        public void Print(string message)
        {
#if DEBUG
            Debug.WriteLine($"Print: {message}");
#endif
            Console.WriteLine($"Print: {message}");
        }

        public void RandomWaitMethod()
        {
            var random = new Random();
            Thread.Sleep(random.Next(3000, 10000));
        }
    }
}