using ConferenceDelegateManagement1234122.Data;
using ConferenceDelegateManagement1234122.Models;
using Microsoft.EntityFrameworkCore;

namespace ConferenceDelegateManagement1234122.Services
{
    public class ConferenceService : IConferenceService
    {
        private readonly ApplicationDbContext _context;

        public ConferenceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Conference>> GetAllConferencesAsync()
        {
            return await _context.Conferences.ToListAsync();
        }

        public async Task<Conference?> GetConferenceByIdAsync(int id)
        {
            return await _context.Conferences.FindAsync(id);
        }

        public async Task<Conference> CreateConferenceAsync(Conference conference)
        {
            _context.Conferences.Add(conference);
            await _context.SaveChangesAsync();
            return conference;
        }

        public async Task<Conference> UpdateConferenceAsync(Conference conference)
        {
            _context.Entry(conference).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return conference;
        }

        public async Task DeleteConferenceAsync(int id)
        {
            var conference = await _context.Conferences.FindAsync(id);
            if (conference != null)
            {
                _context.Conferences.Remove(conference);
                await _context.SaveChangesAsync();
            }
        }
    }
} 