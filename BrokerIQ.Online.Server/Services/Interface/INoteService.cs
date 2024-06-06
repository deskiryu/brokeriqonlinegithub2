using System;
using System.Collections.Generic;

namespace BrokerIQ.Online.Services.Interface
{
    using BrokerIQ.Online.Models;
    using System.Threading.Tasks;

    public interface INoteService
    {
        Task<Note> SaveNote(string message, DateTime? ReminderDate, int customerId);

        Task<Note> UpdateNote(string message, DateTime? ReminderDate, int id);

        Task<IEnumerable<Note>> GetNotesByBrokerId(int customerId);

        Task<bool> Delete(int id);
    }
}
