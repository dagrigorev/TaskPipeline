using System;
using System.Collections.Generic;
using System.Text;

namespace Pipeline
{
    /// <summary>
    /// Pipeline definition
    /// </summary>
    public interface ITaskPipeline
    {
        /// <summary>
        /// Register delegate as a step of pipeline
        /// </summary>
        /// <param name="action"></param>
        void Register(Action action);
        void Register<T>(Action<T> action);
        void Register<T1, T2>(Action<T1, T2> action);
        void Register<T>(Func<T> func);
        void Register<TArg, TResult>(Func<TArg, TResult> func);
        void Register<T1Arg, T2Arg, TResult>(Func<T1Arg, T2Arg, TResult> func);

        /// <summary>
        /// Executes registered action
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actionArg"></param>
        void Execute<T>(T actionArg);

        /// <summary>
        /// Executes registered action with multiple args
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="actionArg1"></param>
        /// <param name="actionArg2"></param>
        void Execute<T1, T2>(T1 actionArg1, T2 actionArg2);

        /// <summary>
        /// Executes registered function with no args
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Execute<T>();

        /// <summary>
        /// Executes registered function with one arg
        /// </summary>
        /// <typeparam name="TArg"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="funcArg"></param>
        /// <returns></returns>
        TResult Execute<TArg, TResult>(TArg funcArg);

        /// <summary>
        /// Executes registered function with multiple args
        /// </summary>
        /// <typeparam name="T1Arg"></typeparam>
        /// <typeparam name="T2Arg"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        TResult Execute<T1Arg, T2Arg, TResult>(T1Arg arg1, T2Arg arg2);
    }
}
