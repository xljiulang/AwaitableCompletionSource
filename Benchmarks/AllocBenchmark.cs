using BenchmarkDotNet.Attributes;
using System.Threading.Tasks;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class AllocBenchmark
    {
        [Benchmark]
        public void TaskCompletionSource_Alloc()
        {
            var source = new TaskCompletionSource<string>();
        }

        [Benchmark]
        public void AwaitableCompletionSource_Alloc()
        {
            using var source = AwaitableCompletionSource.Create<string>();
        }
    }
}
