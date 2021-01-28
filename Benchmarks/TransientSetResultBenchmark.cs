using BenchmarkDotNet.Attributes;
using System.Threading.Tasks;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class TransientSetResultBenchmark
    {
        [Benchmark]
        public async Task TaskCompletionSource_SetResult()
        {
            var source = new TaskCompletionSource<string>();
            // 这里应该模拟其它线程给它设置结果
            // 但是这样不能体现CompletionSource的内存分配
            // 所以使用source.TrySetResult("Result")替代
            // ThreadPool.QueueUserWorkItem(s => ((TaskCompletionSource<string>)s).TrySetResult("Result"), source);
            source.TrySetResult("Result");

            await source.Task;
        }

        [Benchmark]
        public async Task AwaitableCompletionSource_SetResult()
        {
            using var source = AwaitableCompletionSource.Create<string>();
            // 这里应该模拟其它线程给它设置结果
            // 但是这样不能体现CompletionSource的内存分配
            // 所以使用source.TrySetResult("Result")替代
            //ThreadPool.QueueUserWorkItem(s => ((IAwaitableCompletionSource<string>)s).TrySetResult("Result"), source);
            source.TrySetResult("Result");

            await source.Task;
        }
    }
}
