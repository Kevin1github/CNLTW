using System.Collections.Generic;
using ConferenceDelegateManagement1234122.Models;

public interface IDelegateService
{
    List<ConferenceDelegate> GetAllDelegates();
    ConferenceDelegate GetDelegateById(int id);
    void AddDelegate(ConferenceDelegate delegateModel);
    void UpdateDelegate(ConferenceDelegate delegateModel);
    void DeleteDelegate(int id);
}
