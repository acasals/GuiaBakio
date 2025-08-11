using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using GuiaBakio.Models;
using GuiaBakio.Services.Interfaces;
using Microsoft.Maui.Layouts;
using System.Text.RegularExpressions;

namespace GuiaBakio.Services
{
    public class AddImagenPopupService: IAddImagenPopupService
    {
        private MiImagen miImagen=new();
        private byte[] imagenBytes;
        private Entry entryUrl;
        private Button buttonUrl, buttonGuardar, buttonSelect;
        private Image previewImage;
        public async Task<MiImagen?> MostrarAsync()
        {
            var currentPage = Shell.Current?.CurrentPage;

            if (currentPage is null)
                throw new InvalidOperationException("No se pudo obtener la página actual.");

            var popup = new CommunityToolkit.Maui.Views.Popup
            {
                BackgroundColor = Colors.White
            };

            var tcs = new TaskCompletionSource<MiImagen?>();

            // sección URL
            entryUrl = new Entry
            {
                Placeholder = "Pega una URL de imagen o mapa",
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            buttonUrl = new Button
            {
                Text = "Cargar",
                HorizontalOptions = LayoutOptions.Fill,
                Command=new Command(async() => await DetectarCommandAsync())
            };

            //seccion dispositivo
            var labelDispositivo = new Label
            {
                Text = "O selecciona una desde el dispositivo",
                HorizontalOptions = LayoutOptions.Fill
            };

            buttonSelect = new Button
            {
                Text = "Abrir",
                HorizontalOptions = LayoutOptions.Fill,
                Command=new Command(async () => await SeleccionarArchivoCommandAsync())
            };

            //Imagen previa
            previewImage = new Image
            {
                HorizontalOptions = LayoutOptions.Fill,
                Aspect =Aspect.AspectFill,
                IsVisible = false
            };

            //Guardar
            buttonGuardar = new Button
            {
                Text = "Guardar",
                IsEnabled = false,
                HorizontalOptions = LayoutOptions.Fill,
                Command = new Command(async () =>
                {
                    tcs.TrySetResult(miImagen);
                    await popup.CloseAsync();
                })
            };

            var grid = new Grid
            {
                ColumnSpacing = 10,
                WidthRequest = 300,
                RowDefinitions =
                {
                    new RowDefinition{Height = GridLength.Star},
                    new RowDefinition{Height= GridLength.Star},
                    new RowDefinition {Height= GridLength.Auto },
                    new RowDefinition{Height=GridLength.Star}
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(2,GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                }
            };

            grid.Add(entryUrl, 0, 0);
            grid.Add(buttonUrl, 1, 0);
            grid.Add(labelDispositivo, 0, 1);
            grid.Add(buttonSelect, 0, 1);
            grid.Add(previewImage, 0, 2);
            grid.SetColumnSpan(previewImage, 2);
            grid.Add(buttonGuardar, 1, 3);

            popup.Content = grid;
            await currentPage.ShowPopupAsync(popup, new PopupOptions
            {
                OnTappingOutsideOfPopup = () => tcs.TrySetResult(null)
            });
            return await tcs.Task;
        }

        private async Task DetectarCommandAsync()
        {
            try
            {
                string? entrada = entryUrl.Text?.Trim();
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
                    await Shell.Current.DisplayAlert("URL no válida", "La URL no es de Google Maps ni una imagen reconocida.", "OK");
                    return;
                }

                miImagen.Foto = imagenBytes;
                MostrarVistaPrevia(imagenBytes);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"No se pudo cargar la imagen: {ex.Message}", "OK");
            }
        }

        private async Task SeleccionarArchivoCommandAsync()
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
                miImagen.Foto = imagenBytes;

                MostrarVistaPrevia(imagenBytes);
            }
        }

        private static string ExtraerCentroDesdeUrl(string url)
        {
            // Intenta extraer coordenadas del patrón @lat,long
            var coordMatch = Regex.Match(url, @"@(-?\d+\.\d+),(-?\d+\.\d+)");
            if (coordMatch.Success)
            {
                string lat = coordMatch.Groups[1].Value;
                string lng = coordMatch.Groups[2].Value;
                return $"{lat},{lng}";
            }

            // Intenta extraer nombre de lugar del patrón /place/NOMBRE
            var placeMatch = Regex.Match(url, @"/place/([^/]+)");
            if (placeMatch.Success)
            {
                string lugar = placeMatch.Groups[1].Value.Replace("+", " ");
                return Uri.EscapeDataString(lugar);
            }

            return null;
        }

        private static async Task<byte[]> GenerarImagenMapaDesdeUrl(string entrada)
        {
            string apiKey = "TU_API_KEY"; // Reemplaza con tu clave real
            string center = ExtraerCentroDesdeUrl(entrada);

            if (string.IsNullOrEmpty(center))
                throw new ArgumentException("No se pudo extraer una ubicación válida de la URL.");

            string url = $"https://maps.googleapis.com/maps/api/staticmap?center={center}&zoom=14&size=600x400&maptype=roadmap&key={apiKey}";

            using (HttpClient client = new())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                else
                {
                    throw new Exception($"Error al obtener la imagen: {response.StatusCode}");
                }
            }
        }

        private static async Task<byte[]> DescargarImagenDesdeUrl(string url)
        {
            using var httpClient = new HttpClient();
            return await httpClient.GetByteArrayAsync(url);
        }

        private static bool EsUrlDeGoogleMaps(string url)
        {
            return url.Contains("google.com/maps") || url.Contains("goo.gl/maps");
        }

        private static  bool EsUrlDeImagen(string url)
        {
            return url.EndsWith(".jpg") || url.EndsWith(".jpeg") || url.EndsWith(".png") || url.EndsWith(".webp");
        }
       
        private void MostrarVistaPrevia(byte[] bytes)
        {
            if (bytes == null) return;

            previewImage.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
            previewImage.IsVisible = true;
            buttonGuardar.IsEnabled = true;
        }
    }
}
