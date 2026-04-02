using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;

namespace CleanCrow.Helpers
{
    public static class AdminHelper
    {
        public static bool IsRunningAsAdmin()
        {
            try
            {
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        public static void RestartAsAdmin()
        {
            try
            {
                var executable = Process.GetCurrentProcess().MainModule?.FileName;
                
                if (string.IsNullOrEmpty(executable))
                {
                    executable = System.Reflection.Assembly.GetExecutingAssembly().Location;
                }

                var processInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process.Start(processInfo);
            }
            catch
            {
                MessageBox.Show(
                    "Não foi possível reiniciar com privilégios de administrador.\n\n" +
                    "Por favor, execute o programa manualmente como administrador.",
                    "Erro de Privilégio",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}