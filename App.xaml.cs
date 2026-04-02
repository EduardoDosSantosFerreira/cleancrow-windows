using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using CleanCrow.Helpers;
using CleanCrow.Services;
using CleanCrow.ViewModels;
using CleanCrow.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CleanCrow
{
    public partial class App : Application
    {
        private static IServiceProvider? _serviceProvider;
        
        public static IServiceProvider Services => _serviceProvider ?? throw new InvalidOperationException("ServiceProvider nao inicializado");

        public App()
        {
            try
            {
                // Verificar se é administrador
                if (!IsRunningAsAdmin())
                {
                    // Tentar reiniciar como administrador
                    bool reiniciado = RestartAsAdmin();
                    
                    if (reiniciado)
                    {
                        // Fechar a instância atual
                        Current.Shutdown();
                        return;
                    }
                    else
                    {
                        // Se não conseguiu reiniciar, mostrar mensagem e fechar
                        MessageBox.Show(
                            "Este programa requer privilegios de administrador para funcionar.\n\n" +
                            "Clique em OK para tentar novamente ou feche o programa.\n\n" +
                            "Se o problema persistir, clique com o botão direito no arquivo e selecione 'Executar como administrador'.",
                            "CleanCrow - Privilegios Necessarios",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        
                        Current.Shutdown();
                        return;
                    }
                }
                
                // Se chegou aqui, é administrador - continuar normalmente
                InicializarAplicacao();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao inicializar: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        private void InicializarAplicacao()
        {
            // Configurar logging com Serilog
            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CleanCrow", "logs", "cleanCrow-.log");

            var logDirectory = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    logPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("CleanCrow iniciando como administrador...");

            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(Log.Logger, dispose: true);
            });

            services.AddSingleton<ISystemCleanupService, SystemCleanupService>();
            services.AddSingleton<IWingetService, WingetService>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();

            _serviceProvider = services.BuildServiceProvider();
            
            Log.Information("Injecao de dependencia configurada com sucesso");
        }

        private bool IsRunningAsAdmin()
        {
            try
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        private bool RestartAsAdmin()
        {
            try
            {
                string executable = Assembly.GetExecutingAssembly().Location;
                
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = executable,
                    UseShellExecute = true,
                    Verb = "runas",  // Isso faz a janela UAC aparecer
                    WindowStyle = ProcessWindowStyle.Normal
                };
                
                Process? process = Process.Start(psi);
                
                if (process != null)
                {
                    // Pequeno delay para garantir que o processo foi iniciado
                    System.Threading.Thread.Sleep(500);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                // Log do erro
                System.Diagnostics.Debug.WriteLine($"Erro ao reiniciar como admin: {ex.Message}");
                return false;
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                // Se não for administrador, não continua (já foi tratado no construtor)
                if (!IsRunningAsAdmin())
                {
                    return;
                }

                Log.Information("Janela principal iniciando...");

                var mainWindow = _serviceProvider?.GetRequiredService<MainWindow>();
                
                if (mainWindow != null)
                {
                    Log.Information("Janela principal criada, exibindo...");
                    mainWindow.Show();
                }
                else
                {
                    Log.Error("Nao foi possivel criar a janela principal");
                    MessageBox.Show("Erro ao criar a janela principal", "Erro", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro durante a inicializacao da janela principal");
                MessageBox.Show($"Erro ao iniciar aplicacao: {ex.Message}", 
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("CleanCrow finalizado");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}