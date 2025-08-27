using GuiaBakio.Models;

namespace GuiaBakio.Services.Interfaces
{
    public interface IAddImagenPopupService
    {
        Task<Foto?> MostrarAsync();
    }
}
