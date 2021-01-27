using System.Runtime.CompilerServices;

namespace System.Threading.Tasks
{
    /// <summary>
    /// 结果等待对象
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public interface IResultAwaiter<TResult> : ICriticalNotifyCompletion
    {
        /// <summary>
        /// 获取任务是否已完成
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// 获取结果
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        TResult GetResult();
    }
}
