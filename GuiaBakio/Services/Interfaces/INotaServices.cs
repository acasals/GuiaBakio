namespace GuiaBakio.Services.Interfaces
{
    public interface INotaServices
    {
        DataBaseService Db { get; }
        ITextEditorPopupService TextEditor { get; }
        IAddImagenPopupService AddImagen { get; }
        IDialogOKService Dialog { get; }
        IEtiquetaLocalidadEditorPopupService Etiquetas { get; }
        INavigationDataService Navigation { get; }
    }
}
