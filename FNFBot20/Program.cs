using System;
using System.Windows.Forms;

namespace FNFBot20
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var form = new Form1();

            Application.ApplicationExit += (sender, args) =>
            {
                try
                {
                    form.bot?.Shutdown();
                }
                catch
                {
                }
            };

            AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
            {
                try
                {
                    form.bot?.Shutdown();
                }
                catch
                {
                }
            };

            Application.Run(form);

            try
            {
                form.bot?.Shutdown();
            }
            catch
            {
            }
        }
    }
}
