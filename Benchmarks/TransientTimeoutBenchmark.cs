using BenchmarkDotNet.Attributes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class TransientTimeoutBenchmark
    {
        [Benchmark]
        public async Task<string> TaskCompletionSource_WithTimeout()
        {
            var source = new TaskCompletionSource<string>();
            using var cancelSource = new CancellationTokenSource(1000);
            cancelSource.Token.Register(() => source.TrySetResult("timeout"));

            // ThreadPool.QueueUserWorkItem(s => ((TaskCompletionSource<string>)s).TrySetResult("Result"), source);
            source.TrySetResult("Result");

            return await source.Task;
        }

        [Benchmark]
        public async Task<string> AwaitableCompletionSource_WithTimeout()
        {
            using var source = AwaitableCompletionSource.Create<string>();
            source.TrySetResultAfter("timeout", TimeSpan.FromMilliseconds(1000d));

            // ThreadPool.QueueUserWorkItem(s => ((IAwaitableCompletionSource<string>)s).TrySetResult("Result"), source);
            source.TrySetResult("Result");

            return await source.Task;
        }
    }
}
