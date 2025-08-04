using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Services;
using GuiaBakio.Pages;
using GuiaBakio.Services;
using GuiaBakio.Services.Interfaces;
using GuiaBakio.ViewModels;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

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
                Debug.WriteLine($"Database path: {dbPath}");
                return new DataBaseService(dbPath);
            });

            builder.Services.AddSingleton<ITextEditorPopupService, TextEditorPopupService>();
            builder.Services.AddTransient<LocalidadDetalleViewModel>();
            builder.Services.AddTransient<LocalidadPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}