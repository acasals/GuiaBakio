using GuiaBakio.Models;
using System.Collections.ObjectModel;

namespace GuiaBakio.Services.Interfaces
{
    public interface IEtiquetaEditorPopupService
    {
        Task<List<Etiqueta>?> MostrarEditorAsync(ObservableCollection<Etiqueta> listaEtiquetas);
    }
}
