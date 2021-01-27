namespace System.Threading.Tasks
{
    /// <summary>
    /// 可等待的任务
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public interface IAwaitableTask<TResult>
    {
        /// <summary>
        /// 获取结果等待对象
        /// </summary>
        /// <returns></returns>
        IResultAwaiter<TResult> GetAwaiter();
    }
}
