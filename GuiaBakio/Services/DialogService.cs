using GuiaBakio.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuiaBakio.Services
{
    public class DialogService : IDialogService
    {
        public Task ShowAlertAsync(string title, string message, string cancel)
        {
            var currentPage = Shell.Current?.CurrentPage;

            if (currentPage != null)
                return currentPage.DisplayAlert(title, message, cancel);

            return Task.CompletedTask;
        }
    }
}
