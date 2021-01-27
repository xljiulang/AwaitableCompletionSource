﻿using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Threading.Tasks
{
    /// <summary>
    /// 表示可等待的完成源
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <summary>
    /// 表示可重复等待的手动设置结果的任务源
    /// </summary> 
    [DebuggerDisplay("IsCompleted = {IsCompleted}")]
    public class AwaitableCompletionSource<TResult> : ICriticalNotifyCompletion, ICompletionSource<TResult>, IDisposable
    {
        /// <summary>
        /// 池
        /// </summary>
        private static readonly ConcurrentQueue<AwaitableCompletionSource<TResult>> pool = new ConcurrentQueue<AwaitableCompletionSource<TResult>>();

        /// <summary>
        /// 完成标记委托
        /// </summary>
        private static readonly Action callbackCompleted = () => { };


        /// <summary>
        /// 延时timer
        /// </summary>
        private readonly Timer delayTimer;

        /// <summary>
        /// 是否设置了delay
        /// </summary>
        private bool hasDelay = false;

        /// <summary>
        /// 是否已释放
        /// </summary>
        private int isDisposed = 0;


        /// <summary>
        /// 延续的任务
        /// </summary>
        private Action callback;

        /// <summary>
        /// 结果值
        /// </summary>
        private TResult result;

        /// <summary>
        /// 异常值
        /// </summary>
        private Exception exception;



        /// <summary>
        /// 获取任务的结果值类型
        /// </summary>
        public Type ResultType => typeof(TResult);

        /// <summary>
        /// 获取任务是否已完成
        /// </summary>
        public bool IsCompleted => ReferenceEquals(this.callback, callbackCompleted);


        /// <summary>
        /// 可重复等待的手动设置结果的任务源
        /// </summary> 
        private AwaitableCompletionSource()
        {
            this.delayTimer = new Timer(DelayCallback, this, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// 延时回调
        /// </summary>
        /// <param name="state"></param>
        private static void DelayCallback(object state)
        {
            var source = (AwaitableCompletionSource<TResult>)state;
            source.hasDelay = false;
            source.SetCompleted(source.result, source.exception);
        }

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <returns></returns>
        public static AwaitableCompletionSource<TResult> Create()
        {
            if (pool.TryDequeue(out var source) == true)
            {
                source.isDisposed = 0;
                source.callback = null;
                return source;
            }
            return new AwaitableCompletionSource<TResult>();
        }

        /// <summary>
        /// 指定时间后尝试设置结果值
        /// </summary>
        /// <param name="result">结果值</param>
        /// <param name="delay">延时</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void SetExceptionAfter(Exception exception, TimeSpan delay)
        {
            this.exception = exception ?? throw new ArgumentNullException(nameof(exception));
            this.hasDelay = true;
            this.delayTimer.Change(delay, Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// 指定时间后尝试设置结果值
        /// </summary>
        /// <param name="result">结果值</param>
        /// <param name="delay">延时</param> 
        public void SetResultAfter(object result, TimeSpan delay)
        {
            this.SetResultAfter((TResult)result, delay);
        }

        /// <summary>
        /// 指定时间后尝试设置结果值
        /// </summary>
        /// <param name="result">结果值</param>
        /// <param name="delay">延时</param> 
        public void SetResultAfter(TResult result, TimeSpan delay)
        {
            this.result = result;
            this.hasDelay = true;
            this.delayTimer.Change(delay, Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// 尝试设置结果值
        /// </summary>
        /// <param name="result">结果值</param> 
        /// <returns></returns>
        public bool SetResult(object result)
        {
            return this.SetResult((TResult)result);
        }

        /// <summary>
        /// 尝试设置结果值
        /// </summary>
        /// <param name="result">结果值</param> 
        /// <returns></returns>
        public bool SetResult(TResult result)
        {
            return this.SetCompleted(result, exception: default);
        }

        /// <summary>
        /// 尝试设置异常值
        /// </summary>
        /// <param name="exception">异常值</param> 
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public bool SetException(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }
            return this.SetCompleted(result: default, exception);
        }


        /// <summary>
        /// 设置为已完成状态
        /// 只有第一次设置有效
        /// </summary>
        /// <param name="result">结果</param>
        /// <param name="exception">异常</param>
        /// <returns></returns>
        private bool SetCompleted(TResult result, Exception exception)
        {
            if (this.IsCompleted == true)
            {
                return false;
            }

            this.RemoveDelay();

            this.result = result;
            this.exception = exception;

            var continuation = Interlocked.Exchange(ref this.callback, callbackCompleted);
            if (continuation != null)
            {
                ThreadPool.UnsafeQueueUserWorkItem(state => ((Action)state)(), continuation);
            }
            return true;
        }

        /// <summary>
        /// 获取等待对象
        /// </summary>
        /// <returns></returns>
        public AwaitableCompletionSource<TResult> GetAwaiter()
        { 
            if (this.isDisposed != 0)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
            return this;
        }

        /// <summary>
        /// 获取结果
        /// </summary>
        /// <returns></returns>
        public TResult GetResult()
        {
            if (this.IsCompleted == false)
            {
                throw new InvalidOperationException();
            }

            this.callback = null;
            if (this.exception != null)
            {
                throw this.exception;
            }
            return this.result;
        }

        /// <summary>
        /// 完成通知
        /// </summary>
        /// <param name="continuation">延续的任务</param>
        void INotifyCompletion.OnCompleted(Action continuation)
        {
            this.UnsafeOnCompleted(continuation);
        }

        /// <summary>
        /// 完成通知
        /// </summary>
        /// <param name="continuation">延续的任务</param>
        public void UnsafeOnCompleted(Action continuation)
        {
            if (ReferenceEquals(this.callback, callbackCompleted) ||
                  ReferenceEquals(Interlocked.CompareExchange(ref this.callback, continuation, null), callbackCompleted))
            {
                Task.Run(continuation);
            }
        }

        /// <summary>
        /// 回收
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref this.isDisposed, 1, 0) == 0)
            {
                this.RemoveDelay();
                pool.Enqueue(this);
            }
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private void RemoveDelay()
        {
            if (this.hasDelay == true)
            {
                this.hasDelay = false;
                this.delayTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }
    }
}