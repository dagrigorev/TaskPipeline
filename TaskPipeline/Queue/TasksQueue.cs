namespace Pipeline.Queue
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>
    /// Implementation of tasks queue
    /// </summary>
    public class TasksQueue : ITaskQueue
    {
        /// <summary>
        /// Maximum time for waiting task (in milliseconds)
        /// </summary>
        const int MaxTaskWaitTime = 100;

        /// <summary>
        /// Tasks storage
        /// </summary>
        private ConcurrentQueue<Task> _tasksQueue;

        /// <summary>
        /// Collection synchronization root
        /// </summary>
        private object _syncRoot;

        /// <summary>
        /// Default constructor
        /// </summary>
        public TasksQueue()
        {
            _syncRoot = new object();
            _tasksQueue = new ConcurrentQueue<Task>();
        }

        /// <inheritdoc />
        public int Count => _tasksQueue.Count;

        /// <inheritdoc />
        public bool IsSynchronized => true;

        /// <inheritdoc />
        public object SyncRoot => _syncRoot;

        /// <inheritdoc />
        public void CopyTo(Task[] array, int index)
        {
            _tasksQueue.CopyTo(array, index);
        }

        /// <inheritdoc />
        public void CopyTo(Array array, int index)
        {
            // TODO: Check this for optimization
            _tasksQueue.ToArray().CopyTo(array, index);
        }

        /// <inheritdoc />
        public IEnumerator<Task> GetEnumerator()
        {
            return _tasksQueue.GetEnumerator();
        }

        /// <inheritdoc />
        public Task PopTask()
        {
            Task result;
            if (!_tasksQueue.TryDequeue(out result))
            {
                Task.Run(() =>
                {
                    var firstTime = DateTime.Now;
                    while (!_tasksQueue.TryDequeue(out result) && (DateTime.Now - firstTime).Milliseconds > MaxTaskWaitTime) ;
                });
            }

            return result;
        }

        /// <inheritdoc />
        public void PushTask(Task t)
        {
            _tasksQueue.Enqueue(t);
        }

        /// <inheritdoc />
        public void PushTaskOnComplete(Task t)
        {
            if (WaitLastTask())
                PushTask(t);
        }

        /// <inheritdoc />
        public async Task PushTaskOnCompleteAsync(Task t)
        {
            if (await WaitLastTaskAsync())
                PushTask(t);
        }

        /// <inheritdoc />
        public Task GetLastTask()
        {
            var array = ToArray();
            return array[array.Length - 1];
        }

        /// <inheritdoc />
        public Task[] ToArray()
        {
            return _tasksQueue.ToArray();
        }

        /// <inheritdoc />
        public bool TryAdd(Task item)
        {
            _tasksQueue.Enqueue(item);
            return true;
        }

        /// <inheritdoc />
        public bool TryTake([MaybeNullWhen(false)] out Task item)
        {
            return _tasksQueue.TryDequeue(out item);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Destroy tasks queue
        /// </summary>
        public void Dispose()
        {
            WaitLastTask();
            _tasksQueue.Clear();
        }

        /// <summary>
        /// Waits until last task complete
        /// </summary>
        /// <returns>Completion flag for lasdt task</returns>
        private bool WaitLastTask()
        {
            // Gets last task without removing
            // TODO: Check this for omptimization
            var array = ToArray();
            var lastTask = array[array.Length - 1];


            return lastTask.IsCompleted;
        }

        /// <summary>
        /// Waits asynchronously until last task complete
        /// </summary>
        /// <returns></returns>
        private Task<bool> WaitLastTaskAsync()
        {
            return Task.Run(() =>
            {
                return WaitLastTask();
            });
        }

        /// <inheritdoc />
        public void RunAll()
        {
            foreach (var task in ToArray())
                task.Start();
        }

        /// <inheritdoc />
        public int WaitAll()
        {
            int successTasksCount = 0;
            foreach (var task in ToArray())
            {
                task.Wait();
                if (task.IsCompletedSuccessfully)
                    successTasksCount++;
            }
            return successTasksCount;
        }

        private bool WaitTask(Task t)
        {
            var firstTime = DateTime.Now;
            while (!t.IsCompleted && (DateTime.Now - firstTime).Milliseconds > MaxTaskWaitTime) ;
            return t.IsCompletedSuccessfully;
        }
    }
}