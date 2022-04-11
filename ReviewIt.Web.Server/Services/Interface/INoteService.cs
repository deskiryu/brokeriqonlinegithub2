using System;
using System.Collections.Generic;
using System.Text;

namespace ReviewIt.Web.Services.Interface
{
    using ReviewIt.Web.Models;
    using System.Threading.Tasks;

    public interface INoteService
    {
        Task<Note> SaveNote(string message, int customerId);

        Task<Note> UpdateNote(string message, int id);

        Task<IEnumerable<Note>> GetNotesByBrokerId(int customerId);

        Task<bool> Delete(int id);

    }
}
