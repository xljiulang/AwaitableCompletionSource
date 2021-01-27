namespace System.Threading.Tasks
{
    /// <summary>
    /// 定义完成源的接口
    /// </summary>
    public interface ICompletionSource 
    {
        /// <summary>
        /// 获取完成源对应结果类型
        /// </summary>
        Type ResultType { get; }

        /// <summary>
        /// 尝试设置结果值
        /// </summary>
        /// <param name="result">结果值</param>
        /// <returns></returns>
        bool SetResult(object result);

        /// <summary>
        /// 尝试设置异常值
        /// </summary>
        /// <param name="exception">异常值</param>
        /// <returns></returns>
        bool SetException(Exception exception);


        /// <summary>
        /// 指定时间后尝试设置结果值
        /// </summary>
        /// <param name="result">结果值</param>
        /// <param name="delay">延时</param>
        void SetResultAfter(object result, TimeSpan delay);

        /// <summary>
        /// 指定时间后尝试设置异常
        /// </summary>
        /// <param name="exception">异常值</param>
        /// <param name="delay">延时</param>
        void SetExceptionAfter(Exception exception, TimeSpan delay);
    }
}
