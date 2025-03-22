using ConferenceDelegateManagement1234122.Models;
using System.Threading.Tasks;

namespace ConferenceDelegateManagement1234122.Services
{
    public interface IAccountService
    {
        Task<bool> RegisterAsync(ApplicationUser model, string password);
    }
}