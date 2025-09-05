using CommunityToolkit.Maui;
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
                fonts.AddFont("MaterialSymbolsOutlined.ttf", "MaterialSymbols");
            });

            builder.Services.AddSingleton<IDialogOKService, DialogOKService>();
            builder.Services.AddSingleton<IDialogYesNoService, DialogYesNoService>();
            builder.Services.AddSingleton<DataBaseService>(provider =>
            {
                var dbName = "GuiaBakio.db";
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, dbName);
                Debug.WriteLine($"Database path: {dbPath}");
                var dialogService = provider.GetRequiredService<IDialogYesNoService>();
                return new DataBaseService(dbPath, dialogService);
            });
            builder.Services.AddSingleton<ITextEditorPopupService, TextEditorPopupService>();
            builder.Services.AddSingleton<IEtiquetaEditorPopupService, EtiquetaEditorPopupService>();
            builder.Services.AddSingleton<IAddItemPopupService, AddItemPopupService>();
            builder.Services.AddSingleton<IAddImagenPopupService, AddImagenPopupService>();
            builder.Services.AddTransient<ListaLocalidadesViewModel>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<LocalidadViewModel>();
            builder.Services.AddTransient<LocalidadPage>();
            builder.Services.AddTransient<NotaViewModel>();
            builder.Services.AddTransient<NotaPage>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<LoginPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}