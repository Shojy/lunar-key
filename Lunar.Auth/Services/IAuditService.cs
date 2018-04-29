using Microsoft.Azure.KeyVault.Models;

namespace Lunar.Auth.Services
{
    public interface IAuditService
    {
        void LogAction(string action);
        void LogAction(string userId, string action);
    }
}