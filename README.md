# AwaitableCompletionSource
```
<PackageReference Include="AwaitableCompletionSource" Version="1.0.0" />
```
AwaitableCompletionSource在多个场景下可替代TaskCompletionSource，更少的cpu时间和内存分配。

* 支持Singleton，单个实例持续使用；
* 支持Dispose后回收复用，创建实例0分配；
* 支持超时自动设置结果或异常，性能远好于TaskCompletionSource包装增加超时功能； 

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

|                              Method |     Mean |    Error |   StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------------ |---------:|---------:|---------:|-------:|------:|------:|----------:|
|      TaskCompletionSource_SetResult | 39.92 ns | 0.201 ns | 0.179 ns | 0.0229 |     - |     - |      96 B |
| AwaitableCompletionSource_SetResult | 86.19 ns | 0.315 ns | 0.295 ns |      - |     - |     - |         - |


#### SingletonSetResult
单例AwaitableCompletionSource的场景，对于SetResult的使用，AwaitableCompletionSource与TaskCompletionSource的cpu时间相当，内存分配为0。


|                              Method |     Mean |    Error |   StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------------ |---------:|---------:|---------:|-------:|------:|------:|----------:|
|      TaskCompletionSource_SetResult | 41.46 ns | 0.744 ns | 1.180 ns | 0.0229 |     - |     - |      96 B |
| AwaitableCompletionSource_SetResult | 49.30 ns | 0.528 ns | 0.494 ns |      - |     - |     - |         - |



#### Transient超时等待
在网络编程里，请求未必有响应来触发SetResult，或者在指定时间内没有触发SetResult，这时需要加入Timeout机制。

* TaskCompletionSource自身不包含Timeout机制，但可以配合CancellationTokenSource(delay)来实现Timeout机制
* AwaitableCompletionSource直接内置了支持Timeout机制的TrySetResultAfter和TrySetExceptionAfter方法


|                                Method |     Mean |   Error |  StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------------------------------- |---------:|--------:|--------:|-------:|------:|------:|----------:|
|      TaskCompletionSource_WithTimeout | 237.0 ns | 4.76 ns | 5.85 ns | 0.1357 |     - |     - |     568 B |
| AwaitableCompletionSource_WithTimeout | 176.6 ns | 0.83 ns | 0.74 ns |      - |     - |     - |         - |



#### Singleton超时等待
|                                Method |     Mean |   Error |  StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------------------------------- |---------:|--------:|--------:|-------:|------:|------:|----------:|
|      TaskCompletionSource_WithTimeout | 233.1 ns | 4.59 ns | 6.58 ns | 0.1357 |     - |     - |     568 B |
| AwaitableCompletionSource_WithTimeout | 131.5 ns | 1.41 ns | 1.32 ns |      - |     - |     - |         - |
