using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class TelephoneInviteService : ITelephoneInviteService
    {
        private readonly string TelephoneUrl = "TelephoneInvite";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;

        public TelephoneInviteService(IRequestProviderService requestProviderService, IMapper mapper)
        {
            this.requestProviderService = requestProviderService;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<TelephoneInvite>> GetTelephoneInvites()
        {
            var answer = await this.requestProviderService.Get<IEnumerable<TelephoneInviteDto>>(this.TelephoneUrl + $"/invites");
            return this.mapper.Map<IEnumerable<TelephoneInvite>>(answer);
        }

        public async Task<IEnumerable<TelephoneInvite>> GetTelephoneInvitesByBrokerId(int brokerId)
        {
            var answer = await this.requestProviderService.Get<IEnumerable<TelephoneInviteDto>>(this.TelephoneUrl + $"/invites/{brokerId}");
            return this.mapper.Map<IEnumerable<TelephoneInvite>>(answer);
        }

        public async Task<bool> AddTelephoneInvites(CreateTelephoneInviteDto createTelephoneInvite)
        {
            var answer = await this.requestProviderService.Post<CreateTelephoneInviteDto, TelephoneInviteDto>(this.TelephoneUrl, createTelephoneInvite);
            return answer != null;
        }

        public async Task<bool> DeleteTelephoneInvite(int id)
        {
            var answer = await this.requestProviderService.Delete(this.TelephoneUrl, id);
            return answer;
        }

        public async Task<bool> UpdateTelephoneInvites(UpdateEmailInviteDto updateTelephoneInviteDto)
        {
            var answer = await this.requestProviderService.Post<UpdateEmailInviteDto, bool>(this.TelephoneUrl + $"/update", updateTelephoneInviteDto);
            return answer;
        }

        public async Task<bool> SaveTelephoneNotes(int invitationId, string notes)
        {
            var url = this.TelephoneUrl + "/update";
            var telephoneInviteDto = new UpdateTelephoneInviteDto
            {
                Id = invitationId,
                NotesTelphone = notes
            };

            var answer = false;
            try
            {
                answer = await this.requestProviderService.Post<UpdateTelephoneInviteDto, bool>(url, telephoneInviteDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SaveTelephoneNotes: exception {ex.Message}");
            }
            return answer;
        }
    }
}