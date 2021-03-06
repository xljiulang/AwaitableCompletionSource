﻿using BenchmarkDotNet.Attributes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class SingletonTimeoutBenchmark
    {
        private IAwaitableCompletionSource<string> awaitableCompletionSource;

        [GlobalSetup]
        public void Setup()
        {
            awaitableCompletionSource = AwaitableCompletionSource.Create<string>();
        }

        [Benchmark]
        public async Task TaskCompletionSource_WithTimeout()
        {
            var source = new TaskCompletionSource<string>();
            using var cancelSource = new CancellationTokenSource(1000);
            cancelSource.Token.Register(() => source.TrySetResult("timeout"));

            // ThreadPool.QueueUserWorkItem(s => ((TaskCompletionSource<string>)s).TrySetResult("Result"), source);
            source.TrySetResult("Result");

            await source.Task;
        }

        [Benchmark]
        public async Task AwaitableCompletionSource_WithTimeout()
        {
            var source = this.awaitableCompletionSource;
            source.TrySetResultAfter("timeout", TimeSpan.FromMilliseconds(1000d));

            // ThreadPool.QueueUserWorkItem(s => ((IAwaitableCompletionSource<string>)s).TrySetResult("Result"), source);
            source.TrySetResult("Result");

            await source.Task;
        }
    }
}
