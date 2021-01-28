using BenchmarkDotNet.Running;
using System;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<SetResultBenchmark>();
            BenchmarkRunner.Run<TimeoutBenchmark>();
            Console.ReadLine();
        }
    }
}
