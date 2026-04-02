using System;

namespace CleanCrow.Models
{
    public class OperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public static OperationResult Ok(string message) =>
            new OperationResult { Success = true, Message = message };

        public static OperationResult Fail(string message, Exception? ex = null) =>
            new OperationResult { Success = false, Message = message, Exception = ex };
    }
}