using ConferenceDelegateManagement1234122.Data;
using ConferenceDelegateManagement1234122.Models;
using Microsoft.EntityFrameworkCore;

namespace ConferenceDelegateManagement1234122.Services
{
    public class DelegateService : IDelegateService
    {
        private readonly ApplicationDbContext _context;

        public async Task<bool> Register(ApplicationUser model)
        {
            // Thực hiện logic đăng ký (lưu vào DB, xử lý thông tin...)
            return true;
        }

        public DelegateService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ConferenceDelegate> GetAllDelegates()
        {
            return _context.ConferenceDelegates.ToList();
        }

        public ConferenceDelegate? GetDelegateById(int id)
        {
            return _context.ConferenceDelegates.FirstOrDefault(d => d.Id == id);
        }

        public void AddDelegate(ConferenceDelegate delegateModel)
        {
            _context.ConferenceDelegates.Add(delegateModel);
            _context.SaveChanges();
        }

        public void UpdateDelegate(ConferenceDelegate delegateModel)
        {
            _context.ConferenceDelegates.Update(delegateModel);
            _context.SaveChanges();
        }

        public void DeleteDelegate(int id)
        {
            var delegateToRemove = _context.ConferenceDelegates.Find(id);
            if (delegateToRemove != null)
            {
                _context.ConferenceDelegates.Remove(delegateToRemove);
                _context.SaveChanges();
            }
        }
    }
}