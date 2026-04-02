using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CleanCrow.Models;
using Microsoft.Extensions.Logging;

namespace CleanCrow.Services
{
    public class WingetService : IWingetService
    {
        private readonly ILogger<WingetService> _logger;

        public WingetService(ILogger<WingetService> logger)
        {
            _logger = logger;
        }

        public async Task<OperationResult> ExecuteUpdateAsync(Action<int, string>? progressCallback = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    progressCallback?.Invoke(5, "Abrindo PowerShell para atualizacao...");
                    
                    string psScript = CriarScriptPowerShell();
                    string tempScript = Path.Combine(Path.GetTempPath(), "CleanCrow_Update_" + DateTime.Now.Ticks + ".ps1");
                    File.WriteAllText(tempScript, psScript, Encoding.UTF8);
                    
                    progressCallback?.Invoke(20, "Executando atualizacao...");
                    
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -ExecutionPolicy Bypass -WindowStyle Maximized -File \"{tempScript}\"",
                        UseShellExecute = true,
                        Verb = "runas",
                        CreateNoWindow = false
                    };
                    
                    Process? process = Process.Start(psi);
                    if (process != null)
                    {
                        process.WaitForExit();
                    }
                    
                    progressCallback?.Invoke(100, "Atualizacao finalizada");
                    
                    try { File.Delete(tempScript); } catch { }
                    
                    return OperationResult.Ok("Atualizacao concluida. Verifique a janela do PowerShell para detalhes.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro durante atualizacao");
                    return OperationResult.Fail("Erro: " + ex.Message);
                }
            });
        }

        private string CriarScriptPowerShell()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# CleanCrow - Atualizador de Sistema");
            sb.AppendLine("");
            sb.AppendLine("$host.UI.RawUI.WindowTitle = 'CleanCrow - Atualizador de Sistema'");
            sb.AppendLine("Clear-Host");
            sb.AppendLine("");
            sb.AppendLine("Write-Host '========================================' -ForegroundColor Cyan");
            sb.AppendLine("Write-Host '    CLEANCROW - ATUALIZADOR DE SISTEMA' -ForegroundColor Yellow");
            sb.AppendLine("Write-Host '========================================' -ForegroundColor Cyan");
            sb.AppendLine("Write-Host ''");
            sb.AppendLine("");
            sb.AppendLine("# Verificar winget");
            sb.AppendLine("Write-Host '[1] Verificando winget...' -ForegroundColor Yellow");
            sb.AppendLine("$winget = Get-Command winget -ErrorAction SilentlyContinue");
            sb.AppendLine("if (-not $winget) {");
            sb.AppendLine("    Write-Host ''");
            sb.AppendLine("    Write-Host 'ERRO: Winget nao encontrado!' -ForegroundColor Red");
            sb.AppendLine("    Write-Host ''");
            sb.AppendLine("    Write-Host 'Para instalar, acesse:' -ForegroundColor Yellow");
            sb.AppendLine("    Write-Host 'https://apps.microsoft.com/detail/9nblggh4nns1' -ForegroundColor Cyan");
            sb.AppendLine("    Write-Host ''");
            sb.AppendLine("    Read-Host 'Pressione ENTER para sair'");
            sb.AppendLine("    exit 1");
            sb.AppendLine("}");
            sb.AppendLine("Write-Host '[OK] Winget encontrado' -ForegroundColor Green");
            sb.AppendLine("Write-Host ''");
            sb.AppendLine("");
            sb.AppendLine("# Verificar atualizacoes");
            sb.AppendLine("Write-Host '[2] Verificando atualizacoes disponiveis...' -ForegroundColor Yellow");
            sb.AppendLine("Write-Host '========================================' -ForegroundColor Cyan");
            sb.AppendLine("Write-Host ''");
            sb.AppendLine("");
            sb.AppendLine("& winget upgrade --accept-source-agreements --disable-interactivity");
            sb.AppendLine("Write-Host ''");
            sb.AppendLine("");
            sb.AppendLine("# Perguntar se quer continuar");
            sb.AppendLine("$resposta = Read-Host 'Deseja atualizar os programas? (S/N)'");
            sb.AppendLine("if ($resposta -notmatch '^[Ss]$') {");
            sb.AppendLine("    Write-Host ''");
            sb.AppendLine("    Write-Host 'Atualizacao cancelada!' -ForegroundColor Yellow");
            sb.AppendLine("    Read-Host 'Pressione ENTER para sair'");
            sb.AppendLine("    exit 0");
            sb.AppendLine("}");
            sb.AppendLine("");
            sb.AppendLine("# Executar atualizacao");
            sb.AppendLine("Write-Host ''");
            sb.AppendLine("Write-Host '[3] Executando winget upgrade --all...' -ForegroundColor Yellow");
            sb.AppendLine("Write-Host '========================================' -ForegroundColor Cyan");
            sb.AppendLine("Write-Host ''");
            sb.AppendLine("");
            sb.AppendLine("& winget upgrade --all --accept-package-agreements --accept-source-agreements --disable-interactivity");
            sb.AppendLine("");
            sb.AppendLine("Write-Host ''");
            sb.AppendLine("Write-Host '========================================' -ForegroundColor Green");
            sb.AppendLine("Write-Host '    ATUALIZACAO CONCLUIDA!' -ForegroundColor Green");
            sb.AppendLine("Write-Host '========================================' -ForegroundColor Green");
            sb.AppendLine("Write-Host ''");
            sb.AppendLine("Read-Host 'Pressione ENTER para fechar'");
            
            return sb.ToString();
        }
    }
}