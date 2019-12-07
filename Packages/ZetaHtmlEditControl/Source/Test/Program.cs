namespace Test
{
    using System;
    using System.Threading;
    using System.Windows.Forms;
    using ZetaHtmlEditControl.Code.MsHtml;
    using ZetaHtmlEditControl.UI;

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.ThreadException += applicationThreadException;
            Application.SetUnhandledExceptionMode( UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += currentDomainUnhandledException;

            HtmlEditorDesignModeManager.IsDesignMode = false;

            try
            {
                doTest();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new TestFormForScreenshots());
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

        private static void doTest()
        {
            var s = @"<br /><span a=""b"">eins <span>zwei</span> drei <span>vier <br /></span></span>.";
            var t = MsHtmlLegacyFromBadToGoodTranslator.RemoveEmptySpanTags(s);
            var q = @"<br /><span a=""b"">eins zwei drei vier <br /></span>.";

            var b = t == q;
            Console.WriteLine(b);
        }
    }
}