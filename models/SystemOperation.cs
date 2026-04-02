namespace CleanCrow.Models
{
    /// <summary>
    /// Representa uma operação de sistema (limpeza ou atualização)
    /// </summary>
    public class SystemOperation
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int ProgressValue { get; set; }
        public OperationType Type { get; set; }
    }

    public enum OperationType
    {
        Cleanup,
        Update
    }
}