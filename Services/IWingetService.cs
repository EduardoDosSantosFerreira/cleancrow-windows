using System;
using System.Threading.Tasks;
using CleanCrow.Models;

namespace CleanCrow.Services
{
    public interface IWingetService
    {
        Task<OperationResult> ExecuteUpdateAsync(Action<int, string>? progressCallback = null);
    }
}