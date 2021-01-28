using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Threading.Tasks
{
    /// <summary>
    /// 提供完成源的创建
    /// </summary>
    public static class AwaitableCompletionSource
    {
        /// <summary>
        /// 创建一个完成源
        /// </summary>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <returns></returns>
        public static IAwaitableCompletionSource<TResult> Create<TResult>()
        {
            return AwaitableCompletionSourceImpl<TResult>.Create();
        }

        /// <summary>
        /// 表示可等待的完成源
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <summary>
        /// 表示可重复等待的手动设置结果的任务源
        /// </summary> 
        [DebuggerDisplay("IsCompleted = {IsCompleted}")]
        private class AwaitableCompletionSourceImpl<TResult> :
            IAwaitableCompletionSource<TResult>,
            IAwaitableTask<TResult>,
            IResultAwaiter<TResult>
        {
            /// <summary>
            /// 池
            /// </summary>
            private static readonly ConcurrentQueue<AwaitableCompletionSourceImpl<TResult>> pool = new ConcurrentQueue<AwaitableCompletionSourceImpl<TResult>>();

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
            /// 获取任务
            /// </summary>
            IAwaitableTask<TResult> IAwaitableCompletionSource<TResult>.Task => this;


            /// <summary>
            /// 可重复等待的手动设置结果的任务源
            /// </summary> 
            private AwaitableCompletionSourceImpl()
            {
                this.delayTimer = new Timer(DelayCallback, this, Timeout.Infinite, Timeout.Infinite);
            }

            /// <summary>
            /// 延时回调
            /// </summary>
            /// <param name="state"></param>
            private static void DelayCallback(object state)
            {
                var source = (AwaitableCompletionSourceImpl<TResult>)state;
                source.hasDelay = false;
                source.SetCompleted(source.result, source.exception);
            }

            /// <summary>
            /// 创建实例
            /// </summary>
            /// <returns></returns>
            public static AwaitableCompletionSourceImpl<TResult> Create()
            {
                if (pool.TryDequeue(out var source) == true)
                {
                    Interlocked.Exchange(ref source.isDisposed, 0);
                    Interlocked.Exchange(ref source.callback, null);
                    return source;
                }
                return new AwaitableCompletionSourceImpl<TResult>();
            }

            /// <summary>
            /// 指定时间后尝试设置结果值
            /// </summary>
            /// <param name="result">结果值</param>
            /// <param name="delay">延时</param>
            /// <exception cref="ArgumentNullException"></exception>
            public void TrySetExceptionAfter(Exception exception, TimeSpan delay)
            {
                if (ReferenceEquals(this.callback, callbackCompleted))
                {
                    return;
                }

                this.exception = exception ?? throw new ArgumentNullException(nameof(exception));
                this.result = default;
                this.hasDelay = true;
                this.delayTimer.Change(delay, Timeout.InfiniteTimeSpan);
            }

            /// <summary>
            /// 指定时间后尝试设置结果值
            /// </summary>
            /// <param name="result">结果值</param>
            /// <param name="delay">延时</param> 
            public void TrySetResultAfter(object result, TimeSpan delay)
            {
                this.TrySetResultAfter((TResult)result, delay);
            }

            /// <summary>
            /// 指定时间后尝试设置结果值
            /// </summary>
            /// <param name="result">结果值</param>
            /// <param name="delay">延时</param> 
            public void TrySetResultAfter(TResult result, TimeSpan delay)
            {
                if (ReferenceEquals(this.callback, callbackCompleted))
                {
                    return;
                }

                this.result = result;
                this.exception = null;
                this.hasDelay = true;
                this.delayTimer.Change(delay, Timeout.InfiniteTimeSpan);
            }

            /// <summary>
            /// 尝试设置结果值
            /// </summary>
            /// <param name="result">结果值</param> 
            /// <returns></returns>
            public bool TrySetResult(object result)
            {
                return this.TrySetResult((TResult)result);
            }

            /// <summary>
            /// 尝试设置结果值
            /// </summary>
            /// <param name="result">结果值</param> 
            /// <returns></returns>
            public bool TrySetResult(TResult result)
            {
                return this.SetCompleted(result, exception: default);
            }

            /// <summary>
            /// 尝试设置异常值
            /// </summary>
            /// <param name="exception">异常值</param> 
            /// <exception cref="ArgumentNullException"></exception>
            /// <returns></returns>
            public bool TrySetException(Exception exception)
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
            [MethodImpl(MethodImplOptions.NoInlining)]
            private bool SetCompleted(TResult result, Exception exception)
            {
                var continuation = Interlocked.Exchange(ref this.callback, callbackCompleted);
                if (ReferenceEquals(continuation, callbackCompleted))
                {
                    return false;
                }

                if (this.hasDelay == true)
                {
                    this.hasDelay = false;
                    this.delayTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }

                this.result = result;
                this.exception = exception;

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
            public IResultAwaiter<TResult> GetAwaiter()
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
                if (ReferenceEquals(Interlocked.CompareExchange(ref this.callback, null, callbackCompleted), callbackCompleted) == false)
                {
                    throw new InvalidOperationException("Unable to get the result when incomplete");
                }

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
            public void OnCompleted(Action continuation)
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
                    if (this.hasDelay == true)
                    {
                        this.hasDelay = false;
                        this.delayTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    }
                    pool.Enqueue(this);
                }
            }

            /// <summary>
            /// 析构函数
            /// </summary>
            ~AwaitableCompletionSourceImpl()
            {
                this.delayTimer.Dispose();
            }
        }
    }
}