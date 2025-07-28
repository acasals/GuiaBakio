using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuiaBakio.Services
{
    public static class AppForegroundNotifier
    {
        public static event Action? AppResumed;

        public static void NotifyResumed()
        {
            AppResumed?.Invoke();
        }
    }
}
