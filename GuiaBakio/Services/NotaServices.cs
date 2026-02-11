using GuiaBakio.Services.Interfaces;

namespace GuiaBakio.Services
{
    public class NotaServices : INotaServices
    {
        public NotaServices(
            DataBaseService db,
            ITextEditorPopupService textEditor,
            IAddImagenPopupService addImagen,
            IDialogOKService dialog,
            IEtiquetaLocalidadEditorPopupService etiquetas,
            INavigationDataService navigation)
        {
            Db = db;
            TextEditor = textEditor;
            AddImagen = addImagen;
            Dialog = dialog;
            Etiquetas = etiquetas;
            Navigation = navigation;
        }

        public DataBaseService Db { get; }
        public ITextEditorPopupService TextEditor { get; }
        public IAddImagenPopupService AddImagen { get; }
        public IDialogOKService Dialog { get; }
        public IEtiquetaLocalidadEditorPopupService Etiquetas { get; }
        public INavigationDataService Navigation { get; }
    }
}