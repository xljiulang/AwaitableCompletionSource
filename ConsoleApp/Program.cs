using System;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var source = AwaitableCompletionSource<string>.Create();

            source.SetResultAfter("dfdf", TimeSpan.FromSeconds(1));

            // source = AwaitableCompletionSource<string>.Create();
            var rx = source.SetResult("d");
            var r = await source;
             

            Console.ReadLine();
        }


    }
}
