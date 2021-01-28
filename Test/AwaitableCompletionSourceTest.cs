using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Test
{
    public class AwaitableCompletionSourceTest
    {
        [Fact]
        public async Task TrySetResultTest()
        {
            using var source = AwaitableCompletionSource.Create<string>();
            Assert.False(source.Task.GetAwaiter().IsCompleted);
            ThreadPool.QueueUserWorkItem(s => ((IAwaitableCompletionSource<string>)s).TrySetResult("Result"), source);
            var result = await source.Task;
            Assert.Equal("Result", result);
        }

        [Fact]
        public async Task TrySetResult2Test()
        {
            using var source = AwaitableCompletionSource.Create<string>();
            Assert.False(source.Task.GetAwaiter().IsCompleted);
            source.TrySetResult("Result");
            ThreadPool.QueueUserWorkItem(s => ((IAwaitableCompletionSource<string>)s).TrySetResult("Result2"), source);
            var result = await source.Task;
            Assert.Equal("Result", result);
        }

        [Fact]
        public async Task TrySetResultAfterTest()
        {
            using var source = AwaitableCompletionSource.Create<string>();
            Assert.False(source.Task.GetAwaiter().IsCompleted);
            source.TrySetResultAfter("Result", TimeSpan.FromMilliseconds(1));
            var result = await source.Task;
            Assert.Equal("Result", result);
        }

        [Fact]
        public async Task TrySetExceptionTest()
        {
            using var source = AwaitableCompletionSource.Create<string>();
            Assert.False(source.Task.GetAwaiter().IsCompleted);
            ThreadPool.QueueUserWorkItem(s => ((IAwaitableCompletionSource<string>)s).TrySetException(new TimeoutException()), source);

            await Assert.ThrowsAsync<TimeoutException>(async () => await source.Task);
        }

        [Fact]
        public async Task TrySetExceptionAfterTest()
        {
            using var source = AwaitableCompletionSource.Create<string>();
            Assert.False(source.Task.GetAwaiter().IsCompleted);
            source.TrySetExceptionAfter(new TimeoutException(), TimeSpan.FromMilliseconds(1));
            await Assert.ThrowsAsync<TimeoutException>(async () => await source.Task);
        }
    }
}
