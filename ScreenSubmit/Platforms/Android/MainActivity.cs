using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media.Projection;
using Android.OS;
using ScreenSubmit.Platforms.Services;
using System.Diagnostics;

namespace ScreenSubmit
{

    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]

    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // 请求录屏权限
            var mediaProjectionManager = GetSystemService(Context.MediaProjectionService) as MediaProjectionManager;
            var permissionIntent = mediaProjectionManager.CreateScreenCaptureIntent();
            StartActivityForResult(permissionIntent, 100);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == 100 && resultCode == Result.Ok)
            {
                var intent = new Intent(this, typeof(ScreenShotService));
                intent.SetAction("START_RECORDING");
                intent.PutExtra("resultCode", (int)resultCode);
                intent.PutExtra("data", data);
                StartService(intent);
            }
        }
    }
}
