﻿namespace System.Threading.Tasks
{
    /// <summary>
    /// 可等待完成源接口
    /// </summary>
    public interface IAwaitableCompletionSource : IDisposable
    {
        /// <summary>
        /// 获取对应结果类型
        /// </summary>
        Type ResultType { get; }

        /// <summary>
        /// 尝试设置结果值
        /// </summary>
        /// <param name="result">结果值</param>
        /// <returns></returns>
        bool TrySetResult(object result);

        /// <summary>
        /// 尝试设置异常值
        /// </summary>
        /// <param name="exception">异常值</param>
        /// <returns></returns>
        bool TrySetException(Exception exception);


        /// <summary>
        /// 指定时间后尝试设置结果值
        /// </summary>
        /// <param name="result">结果值</param>
        /// <param name="delay">延时</param>
        void TrySetResultAfter(object result, TimeSpan delay);

        /// <summary>
        /// 指定时间后尝试设置异常
        /// </summary>
        /// <param name="exception">异常值</param>
        /// <param name="delay">延时</param>
        void TrySetExceptionAfter(Exception exception, TimeSpan delay);
    }
}
