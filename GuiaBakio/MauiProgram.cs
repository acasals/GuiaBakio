using CommunityToolkit.Maui;
using GuiaBakio.Pages;
using GuiaBakio.Popups;
using GuiaBakio.Services;
using GuiaBakio.Services.Interfaces;
using GuiaBakio.ViewModels;
using Microsoft.Extensions.Logging;
using SQLite;

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

            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton<SQLiteAsyncConnection>(provider =>
            {
                var dbName = "GuiaBakio.db";
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, dbName);
                return new SQLiteAsyncConnection(dbPath);
            });
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<IDialogOKService, DialogOKService>();
            builder.Services.AddSingleton<IDialogYesNoService, DialogYesNoService>();
            builder.Services.AddSingleton<DataBaseService>(provider =>
            {
                var dbName = "GuiaBakio.db";
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, dbName);
                var dialogService = provider.GetRequiredService<IDialogYesNoService>();
                return new DataBaseService(dbPath, dialogService);
            });
            builder.Services.AddSingleton<INavigationDataService, NavigationDataService>();
            builder.Services.AddTransient<ITextEditorPopupService, TextEditorPopupService>();
            builder.Services.AddTransient<IEtiquetaLocalidadEditorPopupService, EtiquetaLocalidadEditorPopupService>();
            builder.Services.AddTransient<AddItemPopup>();
            builder.Services.AddTransient<IAddItemPopupService, AddItemPopupService>();
            builder.Services.AddTransient<IAddImagenPopupService, AddImagenPopupService>();
            builder.Services.AddTransient<ListaNotasViewModel>();
            builder.Services.AddTransient<MainPage>();
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