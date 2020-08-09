namespace Pipeline
{
    /// <summary>
    /// Execution expression contract.
    /// </summary>
    public interface IPipelineItemExecutionExpression
    {
        /// <summary>
        /// Checks that pipeline item can execute.
        /// </summary>
        /// <returns></returns>
        bool CanExecute();

        /// <summary>
        /// Checks that pipeline item can execute.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        bool CanExecute(params object[] args);
    }
}