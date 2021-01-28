using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Test
{
    public class TaskCountTest
    {
        [Fact]
        public async Task TaskCount_100_Test()
        {
            var ran = new Random();
            var sources = Enumerable.Range(0, 100).Select(i => AwaitableCompletionSource.Create<int>()).ToArray();
            foreach (var item in sources)
            {
                // 2秒后没有结果的，自动设置为0
                item.TrySetResultAfter(0, TimeSpan.FromSeconds(2d));
            }

            var resultValue = 2;
            var resultSources = sources.Where(i => ran.Next(0, 1000) <500).ToArray();
            Assert.NotEqual(sources.Length, resultSources.Length);

            foreach (var item in resultSources)
            {
                ThreadPool.QueueUserWorkItem(s => ((IAwaitableCompletionSource)s).TrySetResult(resultValue), item);
            }

            var tasks = sources.Select(a => AwaitableToTask<int>(a));
            var t = await Task.WhenAll(tasks);
            var sum = resultSources.Count() * resultValue;

            foreach (var item in sources)
            {
                item.Dispose();
            }

            Assert.Equal(sum, t.Sum());
        }

        [Fact]
        public async Task TaskCount_1000_Test()
        {
            var ran = new Random();
            var sources = Enumerable.Range(0, 1000).Select(i => AwaitableCompletionSource.Create<int>()).ToArray();
            foreach (var item in sources)
            {
                // 2秒后没有结果的，自动设置为0
                item.TrySetResultAfter(0, TimeSpan.FromSeconds(2d));
            }

            var resultValue = 2;
            var resultSources = sources.Where(i => ran.Next(0, 1000) < 500).ToArray();
            Assert.NotEqual(sources.Length, resultSources.Length);
            foreach (var item in resultSources)
            {
                ThreadPool.QueueUserWorkItem(s => ((IAwaitableCompletionSource)s).TrySetResult(resultValue), item);
            }

            var tasks = sources.Select(a => AwaitableToTask<int>(a));
            var t = await Task.WhenAll(tasks);
            var sum = resultSources.Count() * resultValue;

            foreach (var item in sources)
            {
                item.Dispose();
            }

            Assert.Equal(sum, t.Sum());

        }

        static async Task<T> AwaitableToTask<T>(IAwaitableCompletionSource<T> awaitable)
        {
            return await awaitable.Task;
        }
    }
}
