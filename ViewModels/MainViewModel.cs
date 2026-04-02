using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CleanCrow.Models;
using CleanCrow.Services;
using Microsoft.Extensions.Logging;

namespace CleanCrow.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ISystemCleanupService _cleanupService;
        private readonly IWingetService _wingetService;
        private readonly ILogger<MainViewModel> _logger;

        private bool _isOperationRunning;
        private int _currentProgress;
        private string _progressLabel = "Aguardando inicio da operacao";
        private string _currentOperation = string.Empty;
        private string _statusText = "Pronto";
        private string _statusColor = "#27AE60";
        private int _totalOperations = 45;
        private ObservableCollection<LogMessage> _logs = new();

        public MainViewModel(
            ISystemCleanupService cleanupService,
            IWingetService wingetService,
            ILogger<MainViewModel> logger)
        {
            _cleanupService = cleanupService;
            _wingetService = wingetService;
            _logger = logger;

            StartCleanupCommand = new RelayCommand(_ => _ = StartCleanupAsync(), _ => !IsOperationRunning);
            StartUpdateCommand = new RelayCommand(_ => _ = StartUpdateAsync(), _ => !IsOperationRunning);
            ClearLogsCommand = new RelayCommand(_ => ClearLogs());
        }

        public bool IsOperationRunning
        {
            get => _isOperationRunning;
            set
            {
                _isOperationRunning = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public int CurrentProgress
        {
            get => _currentProgress;
            set
            {
                _currentProgress = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProgressPercentage));
                
                var completedOperations = (int)(value / 100.0 * _totalOperations);
                OperationsCounter = $"{completedOperations}/{_totalOperations}";
            }
        }

        public string ProgressLabel
        {
            get => _progressLabel;
            set { _progressLabel = value; OnPropertyChanged(); }
        }

        public string CurrentOperation
        {
            get => _currentOperation;
            set { _currentOperation = value; OnPropertyChanged(); }
        }

        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        public string StatusColor
        {
            get => _statusColor;
            set { _statusColor = value; OnPropertyChanged(); }
        }

        public string ProgressPercentage => $"{CurrentProgress}%";
        
        private string _operationsCounter = "0/45";
        public string OperationsCounter
        {
            get => _operationsCounter;
            set { _operationsCounter = value; OnPropertyChanged(); }
        }

        public int LogsCount => Logs.Count;

        public ObservableCollection<LogMessage> Logs
        {
            get => _logs;
            set
            {
                _logs = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LogsCount));
            }
        }

        public ICommand StartCleanupCommand { get; }
        public ICommand StartUpdateCommand { get; }
        public ICommand ClearLogsCommand { get; }

        private async Task StartCleanupAsync()
        {
            try
            {
                AddLog("Iniciando limpeza completa do sistema...", LogType.System);
                AddLog("Verificando privilegios de administrador...", LogType.Info);
                
                await ExecuteOperationAsync(
                    (callback) => _cleanupService.ExecuteCleanupAsync(callback),
                    "limpeza");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante a limpeza");
                AddLog($"Erro inesperado: {ex.Message}", LogType.Error);
                StatusText = "Erro";
                StatusColor = "#E74C3C";
                IsOperationRunning = false;
            }
        }

        private async Task StartUpdateAsync()
        {
            try
            {
                AddLog("Iniciando atualizacao completa do sistema...", LogType.System);
                AddLog("Verificando privilegios de administrador...", LogType.Info);
                
                await ExecuteOperationAsync(
                    (callback) => _wingetService.ExecuteUpdateAsync((p, m) => callback(p, m, "")),
                    "atualizacao");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante a atualizacao");
                AddLog($"Erro inesperado: {ex.Message}", LogType.Error);
                StatusText = "Erro";
                StatusColor = "#E74C3C";
                IsOperationRunning = false;
            }
        }

        private async Task ExecuteOperationAsync(
            Func<Action<int, string, string>, Task<OperationResult>> operation, 
            string operationType)
        {
            IsOperationRunning = true;
            CurrentProgress = 0;
            ProgressLabel = $"Iniciando {operationType} do sistema...";
            StatusText = "Executando";
            StatusColor = "#F39C12";

            try
            {
                var result = await operation(UpdateProgress);

                if (result.Success)
                {
                    CurrentProgress = 100;
                    ProgressLabel = "Operacao concluida com sucesso!";
                    StatusText = "Concluido";
                    StatusColor = "#27AE60";
                    
                    // Extrair apenas a mensagem limpa, sem caracteres especiais
                    string cleanMessage = ObterMensagemLimpa(result.Message, operationType);
                    AddLog(cleanMessage, LogType.Success);
                    
                    await Task.Delay(500);
                    
                    // Mostrar mensagem de sucesso limpa
                    string titulo = operationType == "limpeza" ? "Limpeza Concluida" : "Atualizacao Concluida";
                    string icone = operationType == "limpeza" ? "🧹" : "🔄";
                    string mensagemSucesso = $"{icone} {titulo}!\n\n{cleanMessage}";
                    
                    MessageBox.Show(mensagemSucesso, titulo, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    StatusText = "Erro";
                    StatusColor = "#E74C3C";
                    ProgressLabel = "Operacao falhou!";
                    AddLog($"Erro: {result.Message}", LogType.Error);
                    
                    await Task.Delay(500);
                    MessageBox.Show(result.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                IsOperationRunning = false;
                _logger.LogInformation($"Operacao de {operationType} concluida");
            }
        }

        private string ObterMensagemLimpa(string mensagem, string operationType)
        {
            // Se for mensagem de sucesso da limpeza, extrair apenas o essencial
            if (operationType == "limpeza")
            {
                // Procurar por linhas com "Liberado" ou espaço recuperado
                var lines = mensagem.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var mensagemLimpa = new System.Text.StringBuilder();
                
                // Extrair informações úteis
                foreach (var line in lines)
                {
                    string linha = line.Trim();
                    if (linha.Contains("Liberado") || linha.Contains("GB") || linha.Contains("MB") || linha.Contains("KB"))
                    {
                        if (!linha.Contains("╔") && !linha.Contains("║") && !linha.Contains("╚") && 
                            !linha.Contains("═") && !linha.Contains("┌") && !linha.Contains("└"))
                        {
                            mensagemLimpa.AppendLine(linha);
                        }
                    }
                }
                
                if (mensagemLimpa.Length > 0)
                {
                    return $"Operacao concluida com sucesso!\n\n{mensagemLimpa.ToString().Trim()}";
                }
                
                return "Operacao de limpeza concluida com sucesso! O sistema foi otimizado.";
            }
            else
            {
                // Para atualização, extrair programas atualizados
                var lines = mensagem.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var mensagemLimpa = new System.Text.StringBuilder();
                int count = 0;
                
                foreach (var line in lines)
                {
                    string linha = line.Trim();
                    if ((linha.Contains("upgraded") || linha.Contains("atualizado")) && count < 5)
                    {
                        mensagemLimpa.AppendLine($"  • {linha}");
                        count++;
                    }
                }
                
                if (mensagemLimpa.Length > 0)
                {
                    return $"Programas atualizados:\n{mensagemLimpa.ToString().Trim()}";
                }
                
                return "Atualizacao concluida com sucesso! O sistema foi atualizado.";
            }
        }

        private void UpdateProgress(int progress, string operationName, string details)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CurrentProgress = progress;
                CurrentOperation = operationName;
                ProgressLabel = $"Executando: {operationName}";
                
                if (!string.IsNullOrEmpty(details) && details.Length > 10 && 
                    !details.Contains("╔") && !details.Contains("║") && !details.Contains("╚"))
                {
                    AddLog(details, LogType.Info);
                }
            });
        }

        public void AddLog(string message, LogType type)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var log = new LogMessage
                {
                    Timestamp = DateTime.Now,
                    Message = message,
                    Type = type
                };
                
                Logs.Add(log);
                OnPropertyChanged(nameof(LogsCount));
                
                while (Logs.Count > 500)
                    Logs.RemoveAt(0);
            });
        }

        private void ClearLogs()
        {
            Logs.Clear();
            OnPropertyChanged(nameof(LogsCount));
            AddLog("Logs limpos com sucesso!", LogType.Success);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class LogMessage
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; } = string.Empty;
        public LogType Type { get; set; }

        public string FormattedMessage => $"[{Timestamp:HH:mm:ss}] {Message}";
        
        public string Color
        {
            get
            {
                return Type switch
                {
                    LogType.Info => "#3498DB",
                    LogType.Success => "#27AE60",
                    LogType.Warning => "#F39C12",
                    LogType.Error => "#E74C3C",
                    LogType.System => "#9B59B6",
                    _ => "#FFFFFF"
                };
            }
        }
    }

    public enum LogType
    {
        Info,
        Success,
        Warning,
        Error,
        System
    }
}