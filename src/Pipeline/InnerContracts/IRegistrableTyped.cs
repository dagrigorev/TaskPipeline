namespace Pipeline.InnerContracts
{
    /// <summary>
    /// Typed data registrable contract.
    /// </summary>
    public interface IRegistrableTyped : IRegistrable
    {
        /// <summary>
        /// Registers new pipeline item.
        /// </summary>
        /// <param name="item">Item</param>
        void Register<TOut>(IPipelineItem item);

        /// <summary>
        /// Registers new pipeline item.
        /// </summary>
        /// <param name="item">Item</param>
        void Register<TIn, TOut>(IPipelineItem item);

        /// <summary>
        /// Registers new pipeline item.
        /// </summary>
        /// <param name="item">Item</param>
        void Register<T1In, T2In, TOut>(IPipelineItem item);

        /// <summary>
        /// Registers new pipeline item.
        /// </summary>
        /// <param name="item">Item</param>
        void Register<T1In, T2In, T3In, TOut>(IPipelineItem item);

        /// <summary>
        /// Registers new pipeline item.
        /// </summary>
        /// <param name="item">Item</param>
        void Register<T1In, T2In, T3In, T4In, TOut>(IPipelineItem item);
        
        /// <summary>
        /// Registers new pipeline item.
        /// </summary>
        /// <param name="item">Item</param>
        void UnRegister<TOut>(IPipelineItem item);

        /// <summary>
        /// Registers new pipeline item.
        /// </summary>
        /// <param name="item">Item</param>
        void UnRegister<TIn, TOut>(IPipelineItem item);

        /// <summary>
        /// Registers new pipeline item.
        /// </summary>
        /// <param name="item">Item</param>
        void UnRegister<T1In, T2In, TOut>(IPipelineItem item);

        /// <summary>
        /// Registers new pipeline item.
        /// </summary>
        /// <param name="item">Item</param>
        void UnRegister<T1In, T2In, T3In, TOut>(IPipelineItem item);

        /// <summary>
        /// Registers new pipeline item.
        /// </summary>
        /// <param name="item">Item</param>
        void UnRegister<T1In, T2In, T3In, T4In, TOut>(IPipelineItem item);
    }
}