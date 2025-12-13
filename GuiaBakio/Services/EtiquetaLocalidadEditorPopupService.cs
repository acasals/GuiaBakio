using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using GuiaBakio.Helpers;
using GuiaBakio.Models;
using GuiaBakio.Services.Interfaces;
using Microsoft.Maui.Layouts;
using System.Collections.ObjectModel;

namespace GuiaBakio.Services
{
    public class EtiquetaLocalidadEditorPopupService : IEtiquetaLocalidadEditorPopupService
    {
        private readonly DataBaseService _dbService;

        public EtiquetaLocalidadEditorPopupService(DataBaseService dbService)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }
        public async Task<(List<Etiqueta>?, List<Localidad>?)> MostrarEditorAsync(ObservableCollection<Etiqueta> listaEtiquetas, ObservableCollection<Localidad> listaLocalidades)
        {
            var currentPage = (Shell.Current?.CurrentPage) ?? throw new InvalidOperationException("No se pudo obtener la página actual.");

            var todasLasEtiquetas = await _dbService.ObtenerEtiquetasAsync() ?? throw new InvalidOperationException("No se pudieron obtener las etiquetas.");

            foreach (var etiqueta in todasLasEtiquetas)
            {
                if (listaEtiquetas.Any(e => e.Id == etiqueta.Id))
                {
                    etiqueta.IsSelected = true;
                }
            }

            var todasLasLocalidades = await _dbService.ObtenerLocalidadesAsync() ?? throw new InvalidOperationException("No se pudieron obtener las localidades.");

            foreach (var localidad in todasLasLocalidades)
            {
                if (listaLocalidades.Any(e => e.Id == localidad.Id))
                {
                    localidad.IsSelected = true;
                }
            }
            var tcs = new TaskCompletionSource<(List<Etiqueta>?, List<Localidad>?)>();

            var popup = new CommunityToolkit.Maui.Views.Popup
            {
                BackgroundColor = Colors.White,
                Padding = new Thickness(1),
            };

            // etiquetas
            FlexLayout stackEtiquetas = new()
            {
                WidthRequest = 350,
                Direction = FlexDirection.Row,
                //JustifyContent = FlexJustify.Start,
                Wrap = FlexWrap.Wrap,
                Margin = new Thickness(0, 5, 0, 5),
            };

            foreach (var etiqueta in todasLasEtiquetas)
            {
                Border etiquetaBorder = new()
                {
                    BackgroundColor = etiqueta.IsSelected ? Colors.Turquoise : Colors.PaleTurquoise,
                };

                HorizontalStackLayout chip = new()
                {
                    Spacing = 6,
                    Padding = new Thickness(1),
                    Margin = new Thickness(1),
                };

                Label labelIcono = new()
                {
                    Text = etiqueta.Icono,
                    FontFamily = "MaterialSymbols",
                };
                chip.Children.Add(labelIcono);
                Label labelTexto = new()
                {
                    Text = etiqueta.Nombre,
                };
                chip.Children.Add(labelTexto);
                etiquetaBorder.Content = chip;
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) =>
                {
                    etiqueta.IsSelected = !etiqueta.IsSelected;
                    etiquetaBorder.BackgroundColor = etiqueta.IsSelected ? Colors.Turquoise : Colors.PaleTurquoise;
                };
                etiquetaBorder.GestureRecognizers.Add(tapGesture);

                StyleHelper.ApplyStyle(etiquetaBorder, "MyChipStyle");
                stackEtiquetas.Add(etiquetaBorder);
            }


            // localidades
            FlexLayout stackLocalidades = new()
            {
                WidthRequest = 350,
                Direction = FlexDirection.Row,
                //JustifyContent = FlexJustify.Start,
                Wrap = FlexWrap.Wrap,
                Margin = new Thickness(0, 5, 0, 5),
            };

            foreach (var localidad in todasLasLocalidades)
            {
                Border localidadBorder = new()
                {
                    BackgroundColor = localidad.IsSelected ? Colors.LightGray : Colors.White,
                };

                HorizontalStackLayout chip = new()
                {
                    Spacing = 6,
                    Padding = new Thickness(1),
                    Margin = new Thickness(1),
                };

                Label labelTexto = new()
                {
                    Text = localidad.Nombre,
                };
                chip.Children.Add(labelTexto);
                localidadBorder.Content = chip;
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) =>
                {
                    localidad.IsSelected = !localidad.IsSelected;
                    localidadBorder.BackgroundColor = localidad.IsSelected ? Colors.LightGray : Colors.White;
                };
                localidadBorder.GestureRecognizers.Add(tapGesture);

                StyleHelper.ApplyStyle(localidadBorder, "MyChipStyle");
                stackLocalidades.Add(localidadBorder);
            }

            // botones
            var cancelButton = new Button
            {
                Text = "Cancelar",
                HorizontalOptions = LayoutOptions.Start
            };
            StyleHelper.ApplyStyle(cancelButton, "BotonCancelarStyle");
            cancelButton.Clicked += async (_, _) =>
            {
                tcs.TrySetResult((null, null));
                await popup.CloseAsync();
            };
            var saveButton = new Button
            {
                Text = "Guardar",
                HorizontalOptions = LayoutOptions.End
            };
            saveButton.Clicked += async (_, _) =>
            {
                var etiquetasSeleccionadas = new List<Etiqueta>(todasLasEtiquetas.Where(e => e.IsSelected));
                var localidadesSeleccionadas = new List<Localidad>(todasLasLocalidades.Where(l => l.IsSelected));
                tcs.TrySetResult((etiquetasSeleccionadas, localidadesSeleccionadas));
                await popup.CloseAsync();
            };
            var botonesRow = new Grid
            {
                RowDefinitions =
                    {
                        new RowDefinition { Height = GridLength.Auto }
                    },
                ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Star }
                    }
            };
            botonesRow.Add(cancelButton, 0, 0);
            botonesRow.Add(saveButton, 1, 0);

            // layout
            var layout = new StackLayout
            {
                Padding = new Thickness(2),
                Spacing = 10,
                Children =
                {
                    new Label { Text = "Seleccionar etiquetas y localidades", FontAttributes = FontAttributes.Bold, FontSize=18, HorizontalOptions=LayoutOptions.Center },
                    stackEtiquetas,
                    stackLocalidades,
                    botonesRow
                }
            };
            popup.Content = layout;
            await currentPage.ShowPopupAsync(popup, new PopupOptions
            {
                OnTappingOutsideOfPopup = () => tcs.TrySetResult((null, null))
            });
            return await tcs.Task;
        }
    }
}
