using SQLite;

namespace GuiaBakio.Services.Interfaces
{
    public interface IListaNotasServices
    {
        DataBaseService Db { get; }
        IAddItemPopupService AddItem { get; }
        IDialogOKService Dialog { get; }
        INavigationDataService Navigation { get; }
        SQLiteAsyncConnection Sqlite { get; }
        ApiService Api { get; }
        INotaSeleccionPopupService NotaSeleccion { get; }
    }
}
