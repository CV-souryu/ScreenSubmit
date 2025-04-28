using Android.App;
using Android.Runtime;
using Microsoft.Maui.Platform;
using System.Diagnostics;

namespace ScreenSubmit
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {

        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
        //async void LoopScreen()
        //{
        //    Stopwatch sw = Stopwatch.StartNew();
        //    while (true)
        //    {
        //        sw.Restart();
        //        var result = await Screenshot.Default.CaptureAsync();
        //        Console.WriteLine($"截图耗时{sw.ElapsedMilliseconds}");
        //        await Task.Delay(1000);
        //    }

        //}
    }
}
