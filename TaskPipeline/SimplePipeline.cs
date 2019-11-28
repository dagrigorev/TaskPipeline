namespace Pipeline.Runner
{
    using System;
    using System.Threading.Tasks;
    using Pipeline.Queue;

    /// <summary>
    /// Simple pipeline for executinq incoming tasks in queue
    /// </summary>
    public class SimplePipeline : IDisposable
    {
        /// <summary>
        /// Tasks queue
        /// </summary>
        ITaskQueue _tasks;

        /// <summary>
        /// Default constructor
        /// </summary>
        public SimplePipeline()
        {
            // TODO: Initialize class by default
            _tasks = new TasksQueue();
        }

        /// <summary>
        /// Executes pipeline
        /// </summary>
        /// <param name="args">Task arguments</param>
        public void Execute<R>(Func<object[], R> method, params object[] args)
        {
            _tasks.WaitAll();
            _tasks.PushTask(new Task<R>(() =>
            {
                return method.Invoke(args);
            }));

            _tasks.RunAll();
        }

        /// <summary>
        /// Executes pipeline
        /// </summary>
        /// <param name="args">Task arguments</param>
        public void Execute(Action<object[]> method, params object[] args)
        {
            _tasks.WaitAll();
            _tasks.PushTask(new Task(() =>
            {
                method.Invoke(args);
            }));

            _tasks.RunAll();
        }

        /// <summary>
        /// Executes pipeline
        /// </summary>
        /// <param name="args">Task arguments</param>
        public void Execute(Action method)
        {
            _tasks.WaitAll();
            _tasks.PushTask(new Task(() =>
            {
                method.Invoke();
            }));

            _tasks.RunAll();
        }

        public void Dispose()
        {
            _tasks.Dispose();
        }
    }
}
