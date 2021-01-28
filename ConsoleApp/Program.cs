using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var source = AwaitableCompletionSource.Create<string>();

            ThreadPool.QueueUserWorkItem(s => ((IAwaitableCompletionSource)s).TrySetResult("1"), source);
            Console.WriteLine(await source.Task);

            // 支持多次设置获取结果 
            source.TrySetResultAfter("2", TimeSpan.FromSeconds(1d));
            Console.WriteLine(await source.Task);

            // 支持多次设置获取结果 
            source.TrySetResultAfter("3", TimeSpan.FromSeconds(2d));
            Console.WriteLine(await source.Task);

            // 实例使用完成之后，可以进行回收复用
            source.Dispose();


            Console.ReadLine();
        }
    }
}
