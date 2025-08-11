using GuiaBakio.Models;

namespace GuiaBakio.Services.Interfaces
{
    public interface IAddImagenPopupService
    {
        Task<MiImagen?> MostrarAsync();
    }
}
