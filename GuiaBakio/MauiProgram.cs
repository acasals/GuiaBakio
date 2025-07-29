using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using GuiaBakio.Services;

namespace GuiaBakio
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

            builder.Services.AddSingleton<DataBaseService>(provider =>
            {
                var dbName = "GuiaBakio.db";
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, dbName);
                return new DataBaseService(dbPath);
            });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}