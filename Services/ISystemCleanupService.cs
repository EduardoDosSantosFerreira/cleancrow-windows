using System;
using System.Threading.Tasks;
using CleanCrow.Models;

namespace CleanCrow.Services
{
    public interface ISystemCleanupService
    {
        Task<OperationResult> ExecuteCleanupAsync(Action<int, string, string>? progressCallback = null);
    }
}