using ConferenceDelegateManagement1234122.Models;

namespace ConferenceDelegateManagement1234122.Services
{
    public interface IConferenceService
    {
        Task<List<Conference>> GetAllConferencesAsync();
        Task<Conference?> GetConferenceByIdAsync(int id);
        Task<Conference> CreateConferenceAsync(Conference conference);
        Task<Conference> UpdateConferenceAsync(Conference conference);
        Task DeleteConferenceAsync(int id);
    }
} 