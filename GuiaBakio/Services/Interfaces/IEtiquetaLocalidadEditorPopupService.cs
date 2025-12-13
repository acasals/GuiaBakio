using GuiaBakio.Models;
using System.Collections.ObjectModel;

namespace GuiaBakio.Services.Interfaces
{
    public interface IEtiquetaLocalidadEditorPopupService
    {
        Task<(List<Etiqueta>?, List<Localidad>?)> MostrarEditorAsync(ObservableCollection<Etiqueta> listaEtiquetas, ObservableCollection<Localidad> listaLocalidades);
    }
}
