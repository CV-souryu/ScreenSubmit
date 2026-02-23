# ScreenSubmit

本项目提供的一个能够在 Android 设备上面截取屏幕并**返回 byte[1280\*720\*4] 数组** 的示例。

代码中根据谷歌安卓开发指南中调用 `MediaProjectionManager` 申请屏幕录制。

接受到TCP请求 **(非HTTP)** 时通过 `ImageReader` 获取最后一帧图像的缓冲区指针,将指针内数据直接写入TCP返回流中。

当前该工具支持最小SDK 23(Android 6.0)。最大未测试。


## 依赖

 - [.NET 10](https://dotnet.microsoft.com/zh-cn/download/dotnet/10.0)

## 快速上手

 - 连接你的 Android 设备或模拟器

 - 安装 APK

```shell
adb install com.noelle.auto.screensubmit.apk
```

 - 打开APK

 - 自己想法子通过TCP请求 

```shell
telnet 桥接IP 5001
``` 

```shell
adb forward tcp:12345 tcp:5001
telnet 127.0.0.1 12345
``` 




## 数据返回

```CSharp
//外部程序像素缓冲区
byte[] cache = new byte[1280*720*4];
//TCP请求
var netClient = new TcpClient();
//计时
Stopwatch sw = Stopwatch.StartNew();
Console.WriteLine($"TCP请求开始");
//TCP链接
netClient.Connect(IPEndPoint.Parse("192.168.3.65:5001"));
//TCP返回流
var stream = netClient.GetStream();
var readCount = 0;
var readCountCurrent = 0;
//读取直到无法读取
do
{
    readCountCurrent = stream.Read(cache.AsSpan().Slice(readCount));
    readCount += readCountCurrent;
} while (readCountCurrent != 0);
sw.Stop();
//结束
Console.WriteLine($"TCP读取结束:{readCount}:{sw.ElapsedMilliseconds}ms:非0byte({cache.Count(i => i != 0)})");
```

## 参考

[AndroidDeveloper](https://developer.android.com/media/grow/media-projection?hl=zh-cn)

