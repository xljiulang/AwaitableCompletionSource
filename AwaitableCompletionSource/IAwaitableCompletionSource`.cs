namespace System.Threading.Tasks
{
    /// <summary>
    /// 可等待完成源接口
    /// </summary>
    public interface IAwaitableCompletionSource<TResult> : IAwaitableCompletionSource
    {
        /// <summary>
        /// 获取任务
        /// </summary>
        IAwaitableTask<TResult> Task { get; }

        /// <summary>
        /// 尝试设置结果值
        /// </summary>
        /// <param name="result">结果值</param>
        /// <returns></returns>
        bool TrySetResult(TResult result);

        /// <summary>
        /// 指定时间后尝试设置结果值
        /// </summary>
        /// <param name="result">结果值</param>
        /// <param name="delay">延时</param>
        void TrySetResultAfter(TResult result, TimeSpan delay);
    }
}
