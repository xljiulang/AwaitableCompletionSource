using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Threading.Tasks
{
    /// <summary>
    /// 提供可等待的完成源的创建
    /// </summary>
    public static class AwaitableCompletionSource
    {
        private const int False = 0;
        private const int True = 1;

        /// <summary>
        /// 创建一个可等待的完成源
        /// </summary>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <returns></returns>
        public static IAwaitableCompletionSource<TResult> Create<TResult>()
        {
            return Create<TResult>(continueOnCapturedContext: true);
        }

        /// <summary>
        /// 创建一个可等待的完成源
        /// </summary>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <param name="continueOnCapturedContext">是否捕获同步上下文执行延续的任务</param>
        /// <returns></returns>
        public static IAwaitableCompletionSource<TResult> Create<TResult>(bool continueOnCapturedContext)
        {
            return AwaitableCompletionSourceImpl<TResult>.Create(continueOnCapturedContext);
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
            private int isDisposed = False;


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
            /// 同步上下文 
            /// </summary>
            private SynchronizationContext synchronizationContext;


            /// <summary>
            /// 获取任务的结果值类型
            /// </summary>
            public Type ResultType => typeof(TResult);

            /// <summary>
            /// 获取任务是否已完成
            /// </summary>
            public bool IsCompleted => this.callback == callbackCompleted;

            /// <summary>
            /// 获取任务
            /// </summary>
            public IAwaitableTask<TResult> Task => this;


            /// <summary>
            /// 创建实例
            /// </summary>
            /// <param name="continueOnCapturedContext">是否捕获同步上下文执行延续的任务</param>
            /// <returns></returns>
            public static AwaitableCompletionSourceImpl<TResult> Create(bool continueOnCapturedContext)
            {
                if (pool.TryDequeue(out var source) == true)
                {
                    source.isDisposed = False;
                    source.callback = null;
                }
                else
                {
                    source = new AwaitableCompletionSourceImpl<TResult>();
                }

                if (continueOnCapturedContext == true)
                {
                    source.synchronizationContext = SynchronizationContext.Current;
                }
                return source;
            }

            /// <summary>
            /// 可重复等待的手动设置结果的任务源
            /// </summary> 
            private AwaitableCompletionSourceImpl()
            {
                this.delayTimer = new Timer(DelayTimerCallback, this, Timeout.Infinite, Timeout.Infinite);
            }

            /// <summary>
            /// 延时回调
            /// </summary>
            /// <param name="state"></param>
            private static void DelayTimerCallback(object state)
            {
                var source = (AwaitableCompletionSourceImpl<TResult>)state;
                source.hasDelay = false;
                source.SignalCompleted(source.result, source.exception);
            }

            /// <summary>
            /// 指定时间后尝试设置结果值
            /// </summary>
            /// <param name="exception">异常</param>
            /// <param name="delay">延时</param>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentOutOfRangeException"></exception>
            public void TrySetExceptionAfter(Exception exception, TimeSpan delay)
            {
                if (exception == null)
                {
                    throw new ArgumentNullException(nameof(exception));
                }

                if (delay < TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException(nameof(delay));
                }

                if (this.callback == callbackCompleted)
                {
                    return;
                }

                this.result = default;
                this.exception = exception;

                this.hasDelay = true;
                this.delayTimer.Change(delay, Timeout.InfiniteTimeSpan);
            }


            /// <summary>
            /// 指定时间后尝试设置结果值
            /// </summary>
            /// <param name="result">结果值</param>
            /// <param name="delay">延时</param> 
            /// <exception cref="ArgumentOutOfRangeException"></exception>
            public void TrySetResultAfter(TResult result, TimeSpan delay)
            {
                if (delay < TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException(nameof(delay));
                }

                if (this.callback == callbackCompleted)
                {
                    return;
                }

                this.result = result;
                this.exception = null;

                this.hasDelay = true;
                this.delayTimer.Change(delay, Timeout.InfiniteTimeSpan);
            }

            /// <summary>
            /// 指定时间后尝试设置结果值
            /// </summary>
            /// <param name="result">结果值</param>
            /// <param name="delay">延时</param> 
            /// <exception cref="ArgumentOutOfRangeException"></exception>
            public void TrySetResultAfter(object result, TimeSpan delay)
            {
                this.TrySetResultAfter((TResult)result, delay);
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
                return this.SignalCompleted(result, exception: default);
            }

            /// <summary>
            /// 尝试设置异常值
            /// </summary>
            /// <param name="exception">异常值</param> 
            /// <exception cref="ArgumentNullException"></exception>
            /// <returns></returns>
            public bool TrySetException(Exception exception)
            {
                return this.SignalCompleted(result: default, exception ?? new ArgumentNullException(nameof(exception)));
            }


            /// <summary>
            /// 设置为已完成状态
            /// 只有第一次设置有效
            /// </summary>
            /// <param name="result">结果</param>
            /// <param name="exception">异常</param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.NoInlining)]
            private bool SignalCompleted(TResult result, Exception exception)
            {
                var continuation = Interlocked.Exchange(ref this.callback, callbackCompleted);
                if (continuation == callbackCompleted)
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
                    this.ExecuteContinuation(continuation);
                }
                return true;
            }

            /// <summary>
            /// 获取等待对象
            /// </summary>
            /// <returns></returns>
            public IResultAwaiter<TResult> GetAwaiter()
            {
                return this.isDisposed == False ? this : throw new ObjectDisposedException(this.GetType().Name);
            }

            /// <summary>
            /// 获取结果
            /// </summary>
            /// <returns></returns>
            public TResult GetResult()
            {
                if (this.callback != callbackCompleted)
                {
                    throw new InvalidOperationException("Unable to get the result when incomplete");
                }

                this.callback = null;
                return this.exception != null ? throw this.exception : this.result;
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
                if (this.callback == callbackCompleted ||
                    Interlocked.CompareExchange(ref this.callback, continuation, null) == callbackCompleted)
                {
                    this.ExecuteContinuation(continuation);
                }
            }

            /// <summary>
            /// 执行延续任务
            /// </summary>
            /// <param name="continuation">延续任务</param>
            private void ExecuteContinuation(Action continuation)
            {
                if (this.synchronizationContext == null)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(state => ((Action)state)(), continuation);
                }
                else
                {
                    this.synchronizationContext.Post(state => ((Action)state)(), continuation);
                }
            }

            /// <summary>
            /// 回收
            /// </summary>
            public void Dispose()
            {
                if (Interlocked.CompareExchange(ref this.isDisposed, True, False) == False)
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