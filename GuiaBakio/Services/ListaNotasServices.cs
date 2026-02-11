using GuiaBakio.Services.Interfaces;
using SQLite;

namespace GuiaBakio.Services
{
    public class ListaNotasServices : IListaNotasServices
    {
        public ListaNotasServices(
            DataBaseService db,
            IAddItemPopupService addItem,
            IDialogOKService dialog,
            INavigationDataService navigation,
            SQLiteAsyncConnection sqlite,
            ApiService api)
        {
            Db = db;
            AddItem = addItem;
            Dialog = dialog;
            Navigation = navigation;
            Sqlite = sqlite;
            Api = api;
        }

        public DataBaseService Db { get; }
        public IAddItemPopupService AddItem { get; }
        public IDialogOKService Dialog { get; }
        public INavigationDataService Navigation { get; }
        public SQLiteAsyncConnection Sqlite { get; }
        public ApiService Api { get; }
    }
}
