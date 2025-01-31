using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Services.Base;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Server.Services
{
    public class NoteService : BIQService, INoteService
    {
        private readonly string noteUrl = "note";
        private readonly IMapper mapper;

        public NoteService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
            :base(accountService, requestProviderService)
        {
            this.mapper = mapper;
        }

        public async Task<Note> SaveNote(string message, DateTime? reminderDate, int customerId)
        {
            var brokerId = await GetCurrentBrokerId();

            var note = new CreateNoteDto
            {
                Message = message,
                CustomerId = customerId,    
                BrokerId = brokerId,
                ReminderDate = reminderDate
            };
            var answer = await this._requestProviderService.Post<CreateNoteDto, NoteDto>(this.noteUrl, note);
            return this.mapper.Map<Note>(answer);
        }

        public async Task<Note> UpdateNote(string message, DateTime? reminderDate, int id)
        {
            var note = new UpdateNoteDto
            {
                Id = id,
                Message = message,
                ReminderDate = reminderDate
            };
            var answer = await this._requestProviderService.Put<UpdateNoteDto, NoteDto>(this.noteUrl, note);
            return this.mapper.Map<Note>(answer);
        }

        public async Task<IEnumerable<Note>> GetNotesByBrokerId(int customerId)
        {
            var brokerId = await GetCurrentBrokerId();

            var answer = await this._requestProviderService.Get<IEnumerable<NoteDto>>(this.noteUrl + $"/brokerById?customerId={customerId}&brokerId={brokerId}");
            return this.mapper.Map<IEnumerable<Note>>(answer);
        }

        public async Task<bool> Delete(int id)
        {
            var url = this.noteUrl + $"?id={id}";
            return await this._requestProviderService.Delete(url);
        }
    }
}
