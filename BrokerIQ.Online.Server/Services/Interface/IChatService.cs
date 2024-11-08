using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Services.Base;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IChatService
    {
        Task<Chat> Get(int customerId, int brokerId = 0);

        Task<bool> Send(string message, int customerId);

        Task<bool> SendWithDoc(string message, int customerId, ChatDocument chatDocument, bool NoNotification=false);

        Task<bool> SendMultiple(string message, List<int> listCustomerId, int brokerId);

        Task<bool> SendMultipleWithDoc(string message, List<int> listCustomerId, int brokerId, ChatDocument chatDocument);

        Task<bool> SendMultipleAppLink(List<int> listCustomerId, int brokerId);

        Task<ApiResponse<int>> GetUnRead(int customerId, int brokerId = 0);

        Task<bool> SendMultipleVideoLink(string message, List<int> listCustomerId, int brokerId, string videoUrl, string VideoThumbnailData);

        Task<bool> SendMultipleAudioLink(string message, List<int> listCustomerId, int brokerId, string audioUrl);

        Task<Chat> GetPaged(int customerId, int brokerId, int pageNumber = 1, int pageSize = 25);

        Task<bool> SendDraft(string message, int customerId, DateTime toBeSentOn);

        Task<bool> SendDraftWithDoc(string message, int customerId, ChatDocument chatDocument, DateTime toBeSentOn, bool NoNotification = false);
    }
}
