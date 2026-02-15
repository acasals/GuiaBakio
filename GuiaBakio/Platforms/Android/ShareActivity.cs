using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Provider;
using GuiaBakio.Helpers;


namespace GuiaBakio;

[Activity(
    Name = "GuiaBakio.ShareActivity",
    Exported = true,
    Theme = "@android:style/Theme.Translucent.NoTitleBar",
    LaunchMode = LaunchMode.SingleTask
)]
[IntentFilter(
new[] { Intent.ActionSend, Intent.ActionSendMultiple },
Categories = new[] { Intent.CategoryDefault },
DataMimeType = "image/*"
)]
internal class ShareActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        HandleIntent(Intent);
        BringAppToFront();

        Finish();
    }

    private void BringAppToFront()
    {
        var context = Android.App.Application.Context;

        var intent = new Intent(context, typeof(MainActivity));
        intent.AddFlags(ActivityFlags.ReorderToFront);
        intent.AddFlags(ActivityFlags.NewTask);
        intent.AddFlags(ActivityFlags.SingleTop);

        context.StartActivity(intent);
    }

    private void HandleIntent(Intent? intent)
    {
        if (intent == null)
            return;

        if (intent.Action == Intent.ActionSend)
        {
            var uri = intent.GetParcelableExtra(Intent.ExtraStream) as Android.Net.Uri;
            if (uri != null)
            {
                var path = GetRealPathFromUri(uri);
                if (path != null)
                    ShareHandler.ReceiveSharedFiles(new List<string> { path });
            }
        }
        else if (intent.Action == Intent.ActionSendMultiple)
        {
            var uris = intent.GetParcelableArrayListExtra(Intent.ExtraStream)?
                .Cast<Android.Net.Uri>()
                .ToList();

            if (uris != null)
            {
                var paths = uris
                    .Select(GetRealPathFromUri)
                    .Where(p => p != null)
                    .Cast<string>()
                    .ToList();

                ShareHandler.ReceiveSharedFiles(paths);
            }
        }
    }

    private string? GetRealPathFromUri(Android.Net.Uri uri)
    {
        try
        {
            var context = Android.App.Application.Context;

            // Abrimos el stream del contenido
            using var inputStream = context.ContentResolver.OpenInputStream(uri);
            if (inputStream == null)
                return null;

            // Creamos un archivo temporal en la app
            var tempFileName = $"shared_{Guid.NewGuid()}.jpg";
            var tempFilePath = System.IO.Path.Combine(
                context.CacheDir!.AbsolutePath,
                tempFileName
            );

            using var outputStream = System.IO.File.Create(tempFilePath);
            inputStream.CopyTo(outputStream);

            return tempFilePath;
        }
        catch
        {
            return null;
        }
    }

}
