using GuiaBakio.Models;

namespace GuiaBakio.Services.Interfaces
{
    public interface IEtiquetaEditorPopupService
    {
        Task<List<Etiqueta>?> MostrarEditorAsync(int? notaId);
    }
}
