using ConferenceDelegateManagement1234122.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace ConferenceDelegateManagement1234122.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> RegisterAsync(ApplicationUser model, string password)
        {
            var result = await _userManager.CreateAsync(model, password);
            return result.Succeeded;
        }
    }
}