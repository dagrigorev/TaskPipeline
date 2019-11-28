namespace Pipeline.Queue
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    /// <summary>
    /// Concurrent tasks queue definition
    /// </summary>
    public interface ITaskQueue : IProducerConsumerCollection<Task>, IDisposable
    {
        /// <summary>
        /// Push new task in the beginning of queue
        /// </summary>
        /// <param name="t">Addable task</param>
        void PushTask(Task t);

        /// <summary>
        /// Pop last task from queue
        /// </summary>
        /// <returns>Last task that returned by</returns>
        Task PopTask();

        /// <summary>
        /// Push new task when last task is complete
        /// </summary>
        /// <param name="t">Addable task</param>
        void PushTaskOnComplete(Task t);

        /// <summary>
        /// Returns last task in queue without excluding
        /// </summary>
        /// <returns>Last task</returns>
        Task GetLastTask();

        /// <summary>
        /// Runs all tasks in queue
        /// </summary>
        void RunAll();

        /// <summary>
        /// Wait untils all tasks in queue are not complete
        /// </summary>
        /// <returns>Count of successfully executed tasks</returns>
        int WaitAll();
    }
}