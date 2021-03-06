﻿using BenchmarkDotNet.Running;
using System;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<TransientSetResultBenchmark>();
            BenchmarkRunner.Run<SingletonSetResultBenchmark>();
            BenchmarkRunner.Run<TransientTimeoutBenchmark>();
            BenchmarkRunner.Run<SingletonTimeoutBenchmark>();
            Console.ReadLine();
        }
    }
}
