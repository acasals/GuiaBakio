using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using GuiaBakio.Models;
using GuiaBakio.Popups;
using GuiaBakio.Services.Interfaces;
using System.Text.RegularExpressions;

namespace GuiaBakio.Services;

public class AddImagenPopupService : IAddImagenPopupService
{
    private readonly IServiceProvider _services;

    private Foto miImagen = new();
    private byte[]? imagenBytes;

    public AddImagenPopupService(IServiceProvider services)
    {
        _services = services;
    }

    public async Task<Foto?> MostrarAsync()
    {
        var currentPage = Shell.Current?.CurrentPage
            ?? throw new InvalidOperationException("No se pudo obtener la página actual.");

        var popup = _services.GetRequiredService<AddImagenPopup>();
        var tcs = new TaskCompletionSource<Foto?>();

        popup.CargarUrlSolicitado += async (url) => await ProcesarUrl(url, popup);
        popup.SeleccionarArchivoSolicitado += async () => await SeleccionarArchivo(popup);
        popup.GuardarSolicitado += async (_, __) =>
        {
            tcs.TrySetResult(miImagen);
            await popup.CloseAsync();
        };

        await currentPage.ShowPopupAsync(popup, new PopupOptions
        {
            OnTappingOutsideOfPopup = () => tcs.TrySetResult(null)
        });

        return await tcs.Task;
    }

    private async Task ProcesarUrl(string entrada, AddImagenPopup popup)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(entrada)) return;

            if (EsUrlDeGoogleMaps(entrada))
            {
                imagenBytes = await GenerarImagenMapaDesdeUrl(entrada);
                miImagen.EsMapa = true;
                miImagen.UrlMapa = entrada;
            }
            else if (EsUrlDeImagen(entrada))
            {
                imagenBytes = await DescargarImagenDesdeUrl(entrada);
                miImagen.EsMapa = false;
                miImagen.UrlMapa = "";
            }
            else
            {
                await Shell.Current.DisplayAlert("URL no válida",
                    "La URL no es de Google Maps ni una imagen reconocida.", "OK");
                return;
            }

            miImagen.Blob = imagenBytes;
            popup.MostrarPreview(imagenBytes);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error",
                $"No se pudo cargar la imagen: {ex.Message}", "OK");
        }
    }

    private async Task SeleccionarArchivo(AddImagenPopup popup)
    {
        var resultado = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Selecciona una imagen",
            FileTypes = FilePickerFileType.Images
        });

        if (resultado != null)
        {
            using var stream = await resultado.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            imagenBytes = ms.ToArray();

            miImagen.EsMapa = false;
            miImagen.UrlMapa = "";
            miImagen.Blob = imagenBytes;

            popup.MostrarPreview(imagenBytes);
        }
    }

    private static bool EsUrlDeGoogleMaps(string url) =>
        url.Contains("google.com/maps") || url.Contains("goo.gl/maps");

    private static bool EsUrlDeImagen(string url) =>
        url.EndsWith(".jpg") || url.EndsWith(".jpeg") ||
        url.EndsWith(".png") || url.EndsWith(".webp");

    private static async Task<byte[]> DescargarImagenDesdeUrl(string url)
    {
        using var http = new HttpClient();
        return await http.GetByteArrayAsync(url);
    }

    private static async Task<byte[]> GenerarImagenMapaDesdeUrl(string entrada)
    {
        string? apiKey = Helpers.ApiKeyProvider.GetApiKey();
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("La clave de API de Google Maps no está configurada.");

        string center = ExtraerCentroDesdeUrl(entrada)
            ?? throw new ArgumentException("No se pudo extraer una ubicación válida de la URL.");

        string url = $"https://maps.googleapis.com/maps/api/staticmap?center={center}&zoom=14&size=600x400&maptype=roadmap&key={apiKey}";

        using var client = new HttpClient();
        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Error al obtener la imagen: {response.StatusCode}");

        return await response.Content.ReadAsByteArrayAsync();
    }

    private static string? ExtraerCentroDesdeUrl(string url)
    {
        var coordMatch = Regex.Match(url, @"@(-?\d+\.\d+),(-?\d+\.\d+)");
        if (coordMatch.Success)
            return $"{coordMatch.Groups[1].Value},{coordMatch.Groups[2].Value}";

        var placeMatch = Regex.Match(url, @"/place/([^/]+)");
        if (placeMatch.Success)
            return Uri.EscapeDataString(placeMatch.Groups[1].Value.Replace("+", " "));

        return null;
    }
}