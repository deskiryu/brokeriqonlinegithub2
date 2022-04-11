using System;
using System.Collections.Generic;
using System.Text;

namespace ReviewIt.Web.Server.Services
{
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoMapper;
    using Newtonsoft.Json;
    using BrokerIQ.Dto.Models;
    using ReviewIt.Web.Models;
    using ReviewIt.Web.Services.Interface;
    using ReviewIt.Web.Services.Abstract;
    using ReviewIt.Web.Server.Models;

    public class NoteService : INoteService
    {
        private readonly string noteUrl = "note";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;
        private readonly IAccountService accountService;

        public NoteService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
            this.accountService = accountService;
        }

        public async Task<Note> SaveNote(string message, int customerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var brokerId = user.MasterBrokerId;

            var note = new CreateNoteDto
            {
                Message = message,
                CustomerId = customerId,    
                BrokerId = brokerId,
            };
            var answer = await this.requestProviderService.Post<CreateNoteDto, NoteDto>(this.noteUrl, note);
            return this.mapper.Map<Note>(answer);
        }

        public async Task<Note> UpdateNote(string message, int id)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var note = new UpdateNoteDto
            {
                Id = id,
                Message = message,
            };
            var answer = await this.requestProviderService.Put<UpdateNoteDto, NoteDto>(this.noteUrl, note);
            return this.mapper.Map<Note>(answer);
        }

        public async Task<IEnumerable<Note>> GetNotesByBrokerId(int customerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var brokerId = user.MasterBrokerId;

            var answer = await this.requestProviderService.Get<IEnumerable<NoteDto>>(this.noteUrl + $"/brokerById?customerId={customerId}&brokerId={brokerId}");
            return this.mapper.Map<IEnumerable<Note>>(answer);
        }

        public async Task<bool> Delete(int id)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.noteUrl + $"?id={id}";
            return await this.requestProviderService.Delete(url);
        }

    }
}
