# AwaitableCompletionSource
AwaitableCompletionSource在多个场景下可替代TaskCompletionSource，更少的cpu时间和内存分配。

* 支持Singleton，单个实例持续使用；
* 支持Dispose后回收复用，创建实例0分配；
* 支持超时自动设置结果或异常； 

### 如何使用
使用方式与TaskCompletionSource大体一致。但是要使用静态类Create来创建实例，使用完成后Dispose实例。
```
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
```

### Benchmark

#### TransientSetResult
在频繁创建与回收AwaitableCompletionSource的场景，对于SetResult的使用，AwaitableCompletionSource的cpu时间明显高于TaskCompletionSource，但内存分配为0。

``` ini

``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18363.1316 (1909/November2018Update/19H2)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100
  [Host]     : .NET Core 5.0.0 (CoreCLR 5.0.20.51904, CoreFX 5.0.20.51904), X64 RyuJIT
  DefaultJob : .NET Core 5.0.0 (CoreCLR 5.0.20.51904, CoreFX 5.0.20.51904), X64 RyuJIT


```
|                              Method |     Mean |    Error |   StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------------ |---------:|---------:|---------:|-------:|------:|------:|----------:|
|      TaskCompletionSource_SetResult | 39.92 ns | 0.201 ns | 0.179 ns | 0.0229 |     - |     - |      96 B |
| AwaitableCompletionSource_SetResult | 86.19 ns | 0.315 ns | 0.295 ns |      - |     - |     - |         - |


#### SingletonSetResult
单例AwaitableCompletionSource的场景，对于SetResult的使用，AwaitableCompletionSource与TaskCompletionSource的cpu时间相当，内存分配为0。

``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18363.1316 (1909/November2018Update/19H2)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100
  [Host]     : .NET Core 5.0.0 (CoreCLR 5.0.20.51904, CoreFX 5.0.20.51904), X64 RyuJIT
  DefaultJob : .NET Core 5.0.0 (CoreCLR 5.0.20.51904, CoreFX 5.0.20.51904), X64 RyuJIT


```
|                              Method |     Mean |    Error |   StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------------ |---------:|---------:|---------:|-------:|------:|------:|----------:|
|      TaskCompletionSource_SetResult | 41.46 ns | 0.744 ns | 1.180 ns | 0.0229 |     - |     - |      96 B |
| AwaitableCompletionSource_SetResult | 49.30 ns | 0.528 ns | 0.494 ns |      - |     - |     - |         - |



#### 超时等待
在网络编程里，请求未必有响应来触发SetResult，或者在指定时间内没有触发SetResult,这时需要加入Timeout机制。
TaskCompletionSource自制不包含这个功能，但可以配合Task.WhenAll来做到超时的功能，而AwaitableCompletionSource内置了SetResultAfter和SetExceptionAfter方法。
使用瞬态的AwaitableCompletionSource和TaskCompletionSource作超时等待，AwaitableCompletionSource占很大的优势，如果是单例的AwaitableCompletionSource，则优势更明显。

``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18363.1316 (1909/November2018Update/19H2)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100
  [Host]     : .NET Core 5.0.0 (CoreCLR 5.0.20.51904, CoreFX 5.0.20.51904), X64 RyuJIT
  DefaultJob : .NET Core 5.0.0 (CoreCLR 5.0.20.51904, CoreFX 5.0.20.51904), X64 RyuJIT


```
|                                Method |       Mean |    Error |   StdDev |  Gen 0 |  Gen 1 |  Gen 2 | Allocated |
|-------------------------------------- |-----------:|---------:|---------:|-------:|-------:|-------:|----------:|
|      TaskCompletionSource_WithTimeout | 1,814.7 ns | 34.81 ns | 37.25 ns | 0.1163 | 0.0401 | 0.0019 |     720 B |
| AwaitableCompletionSource_WithTimeout |   177.7 ns |  2.48 ns |  2.20 ns | 0.0172 |      - |      - |      72 B |
