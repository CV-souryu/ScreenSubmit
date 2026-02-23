using Android.App;
using Android.Content;
using Android.Hardware.Display;
using Android.Health.Connect.DataTypes.Units;
using Android.Media;
using Android.Media.Projection;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Java.Nio;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ScreenSubmit.Platforms.Services
{
    public class ScreenCaptureCallback : MediaProjection.Callback
    {

    }
    [Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeMediaProjection)]
    public class ScreenShotService : Service
    {
        private const int NOTIFICATION_ID = 1001;
        private const string NOTIFICATION_CHANNEL_ID = "screen_shot_channel";

        private MediaProjectionManager? MediaProjectionManager { get; set; }
        //private MediaRecorder _mediaRecorder;
        private VirtualDisplay? VirtualDisplay { get; set; }
        private MediaProjection? MediaProjection { get; set; }
        ImageReader? ImageReader { get; set; }

        //ImageAvailableListener ImageAvailableListener { get; set; } = new();
        public override IBinder? OnBind(Intent? intent) => null;
        unsafe void StartHttpService()
        {
            var sw = new Stopwatch();
            var listener = new System.Net.Sockets.TcpListener(IPAddress.Any, 5001);
            listener.Start();
            Console.WriteLine("NOELLE:ScreenForward:启动服务器");
            while (true)
            {
                TcpClient? client = null;
                NetworkStream? clientStream = null;
                try
                {
                    client = listener.AcceptTcpClient();
                    Console.WriteLine("NOELLE:ScreenForward:接受到链接");
                    clientStream = client.GetStream();
                    while (client.Connected)
                    {
                        sw.Restart();
                        var imageCache = ImageReader?.AcquireLatestImage();
                        Console.WriteLine($"NOELLE:ScreenForward:处理图像:{sw.ElapsedMilliseconds}");
                        if (imageCache != null)
                        {
                            try
                            {
                                var buffer = imageCache.GetPlanes()?.FirstOrDefault()?.Buffer;
                                if (buffer != null)
                                {
                                    var bufferPtr = buffer.GetDirectBufferAddress();
                                    var bufferSpan = new Span<byte>((void*)bufferPtr, 3686400);
                                    clientStream.Write(bufferSpan);
                                    Console.WriteLine($"NOELLE:ScreenForward:写入完毕:{sw.ElapsedMilliseconds}");
                                }
                            }
                            finally
                            {
                                imageCache.Close();
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"NOELLE:ScreenForward:连接断开:{ex.Message}");
                }
                finally
                {
                    clientStream?.Dispose();
                    client?.Dispose();
                }
                Console.WriteLine("NOELLE:ScreenForward:结束链接");
            }
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            // 启动前台通知
            CreateNotificationChannel();
            var notification = new Notification.Builder(this, NOTIFICATION_CHANNEL_ID)
                .SetContentTitle("屏幕截图中")
                //.SetSmallIcon(Resource.Drawable.appicon)
                .Build();
            StartForeground(NOTIFICATION_ID, notification);

            // 初始化录屏参数
            if (intent.Action == "START_RECORDING")
            {
                var resultCode = intent.GetIntExtra("resultCode", 0);
                var data = intent.GetParcelableExtra("data") as Intent;
                StartRecording(resultCode, data);
            }
            return StartCommandResult.Sticky;
        }

        private void StartRecording(int resultCode, Intent data)
        {
            MediaProjectionManager = GetSystemService(Context.MediaProjectionService) as MediaProjectionManager;
            if ((MediaProjectionManager) == null)
            {
                Toast.MakeText(this, "无法获取屏幕0", ToastLength.Short);
                return;
            }
            MediaProjection = MediaProjectionManager.GetMediaProjection(resultCode, data);
            if ((MediaProjection) == null)
            {
                Toast.MakeText(this, "无法获取屏幕1", ToastLength.Short);
                return;
            }
            ImageReader = ImageReader.NewInstance(1280, 720, (Android.Graphics.ImageFormatType)1, 2);
            //ImageReader.SetOnImageAvailableListener(ImageAvailableListener, null);
            // 配置 MediaRecorder
            //_mediaRecorder = new MediaRecorder();
            //_mediaRecorder.SetAudioSource(AudioSource.Mic);
            //_mediaRecorder.SetVideoSource(VideoSource.Surface);
            //_mediaRecorder.SetOutputFormat(OutputFormat.Mpeg4);
            //_mediaRecorder.SetOutputFile(GetOutputFilePath());
            //_mediaRecorder.SetVideoEncoder(VideoEncoder.H264);
            //_mediaRecorder.SetAudioEncoder(AudioEncoder.Aac);
            //_mediaRecorder.SetVideoSize(1920, 1080);
            //_mediaRecorder.SetVideoEncodingBitRate(8 * 1000 * 1000);
            //_mediaRecorder.Prepare();
            MediaProjection.RegisterCallback(new ScreenCaptureCallback(), null);
            // 创建虚拟显示层
            VirtualDisplay = MediaProjection.CreateVirtualDisplay(
                "ScreenCapture",
                1280,
                720,
                (int)Resources.DisplayMetrics.DensityDpi,
                DisplayFlags.Round,
                ImageReader.Surface,
                null,
                null);
            Task.Factory.StartNew(StartHttpService);
            Toast.MakeText(this, "开始转发屏幕", ToastLength.Short);

            //_mediaRecorder.Start();
        }



        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(
                    NOTIFICATION_CHANNEL_ID,
                    "截图转发服务",
                    NotificationImportance.Default);
                var manager = GetSystemService(NotificationService) as NotificationManager;
                manager.CreateNotificationChannel(channel);
            }
        }

        public override void OnDestroy()
        {
            //_mediaRecorder?.Stop();
            //_mediaRecorder?.Reset();
            VirtualDisplay?.Release();
            MediaProjection?.Stop();
            base.OnDestroy();
        }
    }
    public class ImageAvailableListener : Java.Lang.Object, ImageReader.IOnImageAvailableListener
    {
        //internal bool isReady = true;
        //public byte[] Cache { get; } = new byte[3686400];
        public Stopwatch Stopwatch { get; } = new Stopwatch();
        public ByteBuffer? Buffer { get; private set; }
        public Android.Media.Image? Image { get; private set; }
        public void OnImageAvailable(ImageReader? reader)
        {
            Stopwatch.Restart();

            var image = reader?.AcquireNextImage();
            if (image == null)
                return;
            try
            {
                Buffer = image.GetPlanes()?.FirstOrDefault()?.Buffer;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                image.Close();
            }
        }
    }
}
