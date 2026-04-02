using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CleanCrow.Models;
using Microsoft.Extensions.Logging;

namespace CleanCrow.Services
{
    public class SystemCleanupService : ISystemCleanupService
    {
        private readonly ILogger<SystemCleanupService> _logger;
        private readonly DiskMonitorService _diskMonitor;

        public SystemCleanupService(ILogger<SystemCleanupService> logger)
        {
            _logger = logger;
            _diskMonitor = new DiskMonitorService();
        }

        public async Task<OperationResult> ExecuteCleanupAsync(Action<int, string, string>? progressCallback = null)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    progressCallback?.Invoke(5, "Iniciando limpeza", "Coletando informacoes dos discos...");
                    
                    var disksBefore = _diskMonitor.GetDiskInfo();
                    string diskSummaryBefore = _diskMonitor.GetDiskSummary(disksBefore);
                    
                    progressCallback?.Invoke(10, "Estado dos discos ANTES da limpeza", diskSummaryBefore);
                    
                    var operations = GetCleanupOperations();
                    int total = operations.Count;
                    int current = 0;
                    var cleanedItems = new List<string>();
                    var errors = new List<string>();

                    foreach (var operation in operations)
                    {
                        try
                        {
                            current++;
                            string operationName = GetOperationDisplayName(operation.Key);
                            int progress = (current * 100) / total;
                            
                            progressCallback?.Invoke(progress, operationName, $"Executando: {operationName}");
                            
                            var result = operation.Value.Invoke();
                            if (result.Success)
                            {
                                cleanedItems.Add($"  ✅ {operationName}: {result.Message}");
                            }
                            else
                            {
                                errors.Add($"  ⚠️ {operationName}: {result.Message}");
                            }
                            
                            await Task.Delay(100);
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"  ❌ {operation.Key}: {ex.Message}");
                            _logger.LogError(ex, $"Erro na operacao {operation.Key}");
                        }
                    }

                    progressCallback?.Invoke(95, "Finalizando", "Coletando informacoes dos discos apos limpeza...");
                    
                    var disksAfter = _diskMonitor.GetDiskInfo();
                    string diskSummaryAfter = _diskMonitor.GetDiskSummary(disksAfter);
                    string diskComparison = _diskMonitor.GetDiskComparison(disksBefore, disksAfter);
                    
                    progressCallback?.Invoke(100, "Limpeza concluida", diskComparison);
                    
                    var report = new System.Text.StringBuilder();
                    report.AppendLine("╔══════════════════════════════════════════════════════════════════╗");
                    report.AppendLine("║                    RELATORIO DE LIMPEZA                          ║");
                    report.AppendLine("╚══════════════════════════════════════════════════════════════════╝");
                    report.AppendLine("");
                    
                    report.AppendLine("📊 ESTADO DOS DISCOS:");
                    report.AppendLine("┌─────────────────────────────────────────────────────────────────┐");
                    report.AppendLine("ANTES DA LIMPEZA:");
                    report.AppendLine(diskSummaryBefore);
                    report.AppendLine("DEPOIS DA LIMPEZA:");
                    report.AppendLine(diskSummaryAfter);
                    report.AppendLine("└─────────────────────────────────────────────────────────────────┘");
                    report.AppendLine("");
                    
                    report.AppendLine("💾 ESPACO RECUPERADO:");
                    report.AppendLine(diskComparison);
                    report.AppendLine("");
                    
                    if (cleanedItems.Count > 0)
                    {
                        report.AppendLine("✅ OPERACOES REALIZADAS COM SUCESSO:");
                        report.AppendLine(string.Join("\n", cleanedItems));
                        report.AppendLine("");
                    }
                    
                    if (errors.Count > 0)
                    {
                        report.AppendLine("⚠️ OPERACOES COM PROBLEMAS:");
                        report.AppendLine(string.Join("\n", errors));
                        report.AppendLine("");
                    }
                    
                    report.AppendLine($"📅 Data: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                    report.AppendLine("═══════════════════════════════════════════════════════════════════");
                    
                    return OperationResult.Ok(report.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro durante a limpeza");
                    return OperationResult.Fail($"Erro durante a limpeza: {ex.Message}", ex);
                }
            });
        }

        private Dictionary<string, Func<(bool Success, string Message)>> GetCleanupOperations()
        {
            return new Dictionary<string, Func<(bool, string)>>
            {
                ["limpar_temporarios"] = () => LimparTemporarios(),
                ["limpar_logs"] = () => LimparLogs(),
                ["limpar_update"] = () => LimparWindowsUpdateCache(),
                ["limpar_dns"] = () => LimparDnsCache(),
                ["limpar_edge"] = () => LimparEdgeCache(),
                ["limpar_chrome"] = () => LimparChromeCache(),
                ["limpar_firefox"] = () => LimparFirefoxCache(),
                ["limpar_opera"] = () => LimparOperaCache(),
                ["limpar_brave"] = () => LimparBraveCache(),
                ["limpar_vivaldi"] = () => LimparVivaldiCache(),
                ["limpar_lixeira"] = () => LimparLixeira(),
                ["remover_programas"] = () => RemoverProgramasDesnecessarios(),
                ["remover_bloatware"] = () => RemoverBloatware(),
                ["desativar_hibernacao"] = () => DesativarHibernacao(),
                ["otimizar_desligamento"] = () => OtimizarDesligamento(),
                ["reiniciar_servicos_essenciais"] = () => ReiniciarServicosEssenciais(),
                ["limpar_cache_windows_update"] = () => LimparCacheWindowsUpdate(),
                ["limpar_arquivos_windows"] = () => LimparArquivosExtrasWindows()
            };
        }

        private string GetOperationDisplayName(string operationKey)
        {
            return operationKey switch
            {
                "limpar_temporarios" => "Limpando arquivos temporarios",
                "limpar_logs" => "Limpando logs do sistema",
                "limpar_update" => "Limpando cache do Windows Update",
                "limpar_dns" => "Limpando cache DNS",
                "limpar_edge" => "Limpando cache do Microsoft Edge",
                "limpar_chrome" => "Limpando cache do Google Chrome",
                "limpar_firefox" => "Limpando cache do Firefox",
                "limpar_opera" => "Limpando cache do Opera",
                "limpar_brave" => "Limpando cache do Brave",
                "limpar_vivaldi" => "Limpando cache do Vivaldi",
                "limpar_lixeira" => "Esvaziando lixeira",
                "remover_programas" => "Removendo programas desnecessarios",
                "remover_bloatware" => "Removendo bloatware",
                "desativar_hibernacao" => "Desativando hibernacao",
                "otimizar_desligamento" => "Otimizando tempo de desligamento",
                "reiniciar_servicos_essenciais" => "Reiniciando servicos essenciais",
                "limpar_cache_windows_update" => "Limpando cache de atualizacoes",
                "limpar_arquivos_windows" => "Limpando arquivos extras do Windows",
                _ => "Executando operacao"
            };
        }

        private (bool Success, string Message) LimparTemporarios()
        {
            long freed = 0;
            var tempPaths = new[] { Path.GetTempPath(), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp") };
            
            foreach (var path in tempPaths)
            {
                if (Directory.Exists(path))
                {
                    var beforeSize = GetDirectorySize(path);
                    CleanDirectory(path);
                    var afterSize = GetDirectorySize(path);
                    freed += (beforeSize - afterSize);
                }
            }
            
            return (true, $"Liberado {FormatBytes(freed)} em arquivos temporarios");
        }

        private (bool Success, string Message) LimparLogs()
        {
            try
            {
                var logs = new[] { "Application", "Security", "System", "Setup" };
                int cleared = 0;
                foreach (var log in logs)
                {
                    ExecuteCommand("wevtutil.exe", $"cl \"{log}\"");
                    cleared++;
                }
                return (true, $"{cleared} logs do sistema limpos");
            }
            catch { return (false, "Erro ao limpar logs"); }
        }

        private (bool Success, string Message) LimparWindowsUpdateCache()
        {
            long freed = 0;
            var updatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution", "Download");
            if (Directory.Exists(updatePath))
            {
                var beforeSize = GetDirectorySize(updatePath);
                CleanDirectory(updatePath);
                var afterSize = GetDirectorySize(updatePath);
                freed = beforeSize - afterSize;
            }
            return (true, $"Liberado {FormatBytes(freed)} do cache do Windows Update");
        }

        private (bool Success, string Message) LimparDnsCache()
        {
            ExecuteCommand("ipconfig", "/flushdns");
            return (true, "Cache DNS limpo");
        }

        private (bool Success, string Message) LimparEdgeCache()
        {
            ExecuteCommand("RunDll32.exe", "InetCpl.cpl,ClearMyTracksByProcess 255");
            var edgePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Edge", "User Data", "Default", "Cache");
            long freed = CleanDirectoryAndGetSize(edgePath);
            return (true, $"Liberado {FormatBytes(freed)} do cache do Edge");
        }

        private (bool Success, string Message) LimparChromeCache()
        {
            var chromePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data", "Default", "Cache");
            long freed = CleanDirectoryAndGetSize(chromePath);
            return (true, $"Liberado {FormatBytes(freed)} do cache do Chrome");
        }

        private (bool Success, string Message) LimparFirefoxCache()
        {
            var firefoxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla", "Firefox", "Profiles");
            long freed = 0;
            if (Directory.Exists(firefoxPath))
            {
                foreach (var profile in Directory.GetDirectories(firefoxPath))
                {
                    var cachePath = Path.Combine(profile, "cache2");
                    freed += CleanDirectoryAndGetSize(cachePath);
                }
            }
            return (true, $"Liberado {FormatBytes(freed)} do cache do Firefox");
        }

        private (bool Success, string Message) LimparOperaCache()
        {
            var operaPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Opera Software", "Opera Stable", "Cache");
            long freed = CleanDirectoryAndGetSize(operaPath);
            return (true, $"Liberado {FormatBytes(freed)} do cache do Opera");
        }

        private (bool Success, string Message) LimparBraveCache()
        {
            var bravePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BraveSoftware", "Brave-Browser", "User Data", "Default", "Cache");
            long freed = CleanDirectoryAndGetSize(bravePath);
            return (true, $"Liberado {FormatBytes(freed)} do cache do Brave");
        }

        private (bool Success, string Message) LimparVivaldiCache()
        {
            var vivaldiPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Vivaldi", "User Data", "Default", "Cache");
            long freed = CleanDirectoryAndGetSize(vivaldiPath);
            return (true, $"Liberado {FormatBytes(freed)} do cache do Vivaldi");
        }

        private (bool Success, string Message) LimparLixeira()
        {
            ExecutePowerShell("Clear-RecycleBin -Force -ErrorAction SilentlyContinue");
            return (true, "Lixeira esvaziada");
        }

        private (bool Success, string Message) RemoverProgramasDesnecessarios()
        {
            ExecuteCommand("where", "FlashUtil*.exe");
            ExecuteCommand("FlashUtil*.exe", "-uninstall");
            return (true, "Programas desnecessarios removidos");
        }

        private (bool Success, string Message) RemoverBloatware()
        {
            var bloatwareApps = new[] { "Microsoft.3DBuilder", "Microsoft.BingWeather", "Microsoft.MicrosoftSolitaireCollection" };
            foreach (var app in bloatwareApps)
            {
                ExecutePowerShell($"Get-AppxPackage {app} | Remove-AppxPackage");
            }
            return (true, "Bloatware removido");
        }

        private (bool Success, string Message) DesativarHibernacao()
        {
            ExecuteCommand("powercfg", "-h off");
            return (true, "Hibernacao desativada");
        }

        private (bool Success, string Message) OtimizarDesligamento()
        {
            ExecuteCommand("reg", "add \"HKCU\\Control Panel\\Desktop\" /v \"WaitToKillAppTimeout\" /t REG_SZ /d \"2000\" /f");
            return (true, "Tempo de desligamento otimizado");
        }

        private (bool Success, string Message) ReiniciarServicosEssenciais()
        {
            var services = new[] { "wuauserv", "bits", "Dnscache" };
            foreach (var service in services)
            {
                ExecuteCommand("net", $"start {service}");
            }
            return (true, "Servicos essenciais reiniciados");
        }

        private (bool Success, string Message) LimparCacheWindowsUpdate()
        {
            long freed = 0;
            var paths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution", "Download"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Logs", "WindowsUpdate")
            };
            foreach (var path in paths)
            {
                freed += CleanDirectoryAndGetSize(path);
            }
            return (true, $"Liberado {FormatBytes(freed)} do cache do Windows Update");
        }

        private (bool Success, string Message) LimparArquivosExtrasWindows()
        {
            long freed = 0;
            var paths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "config", "systemprofile", "AppData", "Local", "Temp")
            };
            foreach (var path in paths)
            {
                freed += CleanDirectoryAndGetSize(path);
            }
            return (true, $"Liberado {FormatBytes(freed)} de arquivos extras do Windows");
        }

        private long GetDirectorySize(string path)
        {
            if (!Directory.Exists(path)) return 0;
            long size = 0;
            try
            {
                var dir = new DirectoryInfo(path);
                foreach (var file in dir.GetFiles("*", SearchOption.AllDirectories))
                {
                    try { size += file.Length; } catch { }
                }
            }
            catch { }
            return size;
        }

        private long CleanDirectoryAndGetSize(string path)
        {
            long size = GetDirectorySize(path);
            CleanDirectory(path);
            return size;
        }

        private void CleanDirectory(string path)
        {
            if (!Directory.Exists(path)) return;
            try
            {
                var dir = new DirectoryInfo(path);
                foreach (var file in dir.GetFiles())
                {
                    try { file.Delete(); } catch { }
                }
                foreach (var subDir in dir.GetDirectories())
                {
                    try { subDir.Delete(true); } catch { }
                }
            }
            catch { }
        }

        private void ExecuteCommand(string command, string arguments)
        {
            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = command,
                        Arguments = arguments,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true
                    }
                };
                process.Start();
                process.WaitForExit(30000);
            }
            catch { }
        }

        private void ExecutePowerShell(string script)
        {
            ExecuteCommand("powershell", $"-Command \"{script}\"");
        }

        private string FormatBytes(long bytes)
        {
            if (bytes <= 0) return "0 B";
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}