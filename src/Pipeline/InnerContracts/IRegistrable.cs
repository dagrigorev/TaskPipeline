namespace Pipeline.InnerContracts
{
    /// <summary>
    /// Registrable pipeline contract.
    /// </summary>
    public interface IRegistrable
    {
        /// <summary>
        /// Registers new pipeline item.
        /// </summary>
        /// <param name="expression">Execution expression.</param>
        /// <param name="item">Item</param>
        void Register(IPipelineItemExecutionExpression expression, IPipelineItem item);

        /// <summary>
        /// Registers new pipeline item.
        /// </summary>
        /// <param name="item">Item</param>
        void UnRegister(IPipelineItem item);
    }
}