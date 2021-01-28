using BenchmarkDotNet.Attributes;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class SetResultBenchmark
    {
        [Benchmark]
        public async Task TaskCompletionSource_SetResult()
        {
            var source = new TaskCompletionSource<string>();
            ThreadPool.QueueUserWorkItem(s => ((TaskCompletionSource<string>)s).TrySetResult("Result"), source);
            await source.Task;
        }

        [Benchmark]
        public async Task AwaitableCompletionSource_SetResult()
        {
            using var source = AwaitableCompletionSource.Create<string>();
            ThreadPool.QueueUserWorkItem(s => ((IAwaitableCompletionSource<string>)s).TrySetResult("Result"), source);
            await source.Task;
        }
    }
}
