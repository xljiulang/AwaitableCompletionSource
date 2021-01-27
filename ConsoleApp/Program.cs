using System;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var source = AwaitableCompletionSource.Create<string>();

            source.TrySetResult("1");
            Console.WriteLine(await source.Task);

            // 支持多次设置获取结果
            source.TrySetResultAfter("2", TimeSpan.FromSeconds(1d));
            Console.WriteLine(await source.Task);

            // 实例使用完成之后，可以进行回收复用
            source.Dispose();


            Console.ReadLine();
        }
    }
}
