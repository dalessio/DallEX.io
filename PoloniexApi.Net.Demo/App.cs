using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DallEX.io.View
{
    public sealed partial class App : Application, IDisposable
    {
        private MainWindow wnd = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            wnd = new MainWindow();
            wnd.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Dispose();
        }


        #region IDisposable Support
        private bool disposedValue = false;

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        if (wnd != null)
                            wnd.Close();

                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        wnd = null;
                        GC.Collect();
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

    }
}
