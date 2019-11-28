namespace Pipeline.Runner
{
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Pipeline;
    using Pipeline.Queue;

    public class TypedPipeline : IDisposable
    {
        /// <summary>
        /// Tasks queue
        /// </summary>
        IDictionary<SignatureKey, ITaskQueue> _tasks;

        /// <summary>
        /// Default constructor
        /// </summary>
        public TypedPipeline()
        {
            // TODO: Initialize class by default
            _tasks = new Dictionary<SignatureKey, ITaskQueue>();
        }

        /// <summary>
        /// Executes pipeline
        /// </summary>
        /// <param name="args">Task arguments</param>
        public void Execute<R>(Func<object[], R> method, params object[] args)
        {
            var queue = _tasks[SignatureKey.GetSignature(method)];
            queue.WaitAll();
            queue.PushTask(new Task<R>(() =>
            {
                return method.Invoke(args);
            }));

            queue.RunAll();
        }

        /// <summary>
        /// Executes pipeline
        /// </summary>
        /// <param name="args">Task arguments</param>
        public void Execute(Action<object[]> method, params object[] args)
        {
            var queue = _tasks[SignatureKey.GetSignature(method)];
            queue.WaitAll();
            queue.PushTask(new Task(() =>
            {
                method.Invoke(args);
            }));

            queue.RunAll();
        }

        /// <summary>
        /// Executes pipeline
        /// </summary>
        public void Execute(Action method)
        {
            var queue = _tasks[SignatureKey.GetSignature(method)];
            queue.WaitAll();
            queue.PushTask(new Task(() =>
            {
                method.Invoke();
            }));

            queue.RunAll();
        }

        public void Dispose()
        {
            // TODO: Check this
            foreach (var value in _tasks.Values)
                value.Dispose();
            _tasks.Clear();
        }
    }
}