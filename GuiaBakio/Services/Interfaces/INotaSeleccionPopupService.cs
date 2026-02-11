using GuiaBakio.Models;


namespace GuiaBakio.Services.Interfaces
{
    public interface INotaSeleccionPopupService
    {
        Task<Nota?> MostrarAsync(IEnumerable<Nota> notas);
    }


}
