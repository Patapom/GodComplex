namespace Test
{
    using System;
    using System.Threading;
    using System.Windows.Forms;

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.ThreadException += applicationThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += currentDomainUnhandledException;

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var form = new MainForm();

                WinHook.OnDoubleCick = form.OnMouseDblClick;
                WinHook.Init();

                Application.Run(form);
            }
            catch (Exception e)
            {
                doHandleException(e);
            }
        }

        private static void doHandleException(Exception x)
        {
            if (x is ObjectDisposedException)
            {
                // Eat.
            }
            else
            {
                if (!x.Message.Contains("HRESULT: 0x80040100 (DRAGDROP_E_NOTREGISTERED)"))
                    MessageBox.Show(x.Message);
            }
        }

        private static void currentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            doHandleException((Exception)e.ExceptionObject);
        }

        private static void applicationThreadException(object sender, ThreadExceptionEventArgs e)
        {
            doHandleException(e.Exception);
        }
    }
}