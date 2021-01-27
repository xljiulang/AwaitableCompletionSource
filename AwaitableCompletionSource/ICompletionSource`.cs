namespace System.Threading.Tasks
{
    /// <summary>
    /// 定义泛型完成源接口
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public interface ICompletionSource<TResult> : ICompletionSource
    {
        /// <summary>
        /// 尝试设置结果值
        /// </summary>
        /// <param name="result">结果值</param>
        /// <returns></returns>
        bool SetResult(TResult result);

        /// <summary>
        /// 指定时间后尝试设置结果值
        /// </summary>
        /// <param name="result">结果值</param>
        /// <param name="delay">延时</param>
        void SetResultAfter(TResult result, TimeSpan delay);
    }
}
