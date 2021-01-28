using BenchmarkDotNet.Attributes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class TimeoutBenchmark
    {
        [Benchmark]
        public async Task<string> TaskCompletionSource_WithTimeout()
        {
            var source = new TaskCompletionSource<string>();
            using var cancelSource = new CancellationTokenSource();
            var delayTask = Task.Delay(1000, cancelSource.Token);

            ThreadPool.QueueUserWorkItem(s => ((TaskCompletionSource<string>)s).TrySetResult("Result"), source);

            var task = await Task.WhenAny(source.Task, delayTask);
            if (task == delayTask)
            {
                return "timeout";
            }
            else
            {
                return await source.Task;
            }
        }

        [Benchmark]
        public async Task<string> AwaitableCompletionSource_WithTimeout()
        {
            using var source = AwaitableCompletionSource.Create<string>();
            source.TrySetResultAfter("timeout", TimeSpan.FromMilliseconds(1000d));

            ThreadPool.QueueUserWorkItem(s => ((IAwaitableCompletionSource)s).TrySetResult("Result"), source);

            return await source.Task;
        }
    }
}
