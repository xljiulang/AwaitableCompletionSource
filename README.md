# AwaitableCompletionSource
可重复使用、可回收复用的TaskCompletionSource方案，超低的内存分配

### 创建或分配实例
``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18363.1316 (1909/November2018Update/19H2)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100
  [Host]     : .NET Core 5.0.0 (CoreCLR 5.0.20.51904, CoreFX 5.0.20.51904), X64 RyuJIT
  DefaultJob : .NET Core 5.0.0 (CoreCLR 5.0.20.51904, CoreFX 5.0.20.51904), X64 RyuJIT


```
|                          Method |     Mean |    Error |   StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------------------------- |---------:|---------:|---------:|-------:|------:|------:|----------:|
|      TaskCompletionSource_Alloc | 10.23 ns | 0.185 ns | 0.164 ns | 0.0229 |     - |     - |      96 B |
| AwaitableCompletionSource_Alloc | 30.91 ns | 0.146 ns | 0.136 ns |      - |     - |     - |         - |

### 超时等待
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
