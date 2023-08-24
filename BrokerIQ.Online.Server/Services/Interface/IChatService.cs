using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Services.Interface
{
    using Dto.Models;
    using Models;
    using BrokerIQ.Dto.Request;

    public interface IChatService
    {
        Task<Chat> Get(int customerId, int brokerId = 0);

        Task<bool> Send(string message, int customerId);

        Task<bool> SendWithDoc(string message, int customerId, ChatDocument chatDocument, bool NoNotification=false);

        Task<bool> SendMultiple(string message, List<int> listCustomerId, int brokerId);

        Task<bool> SendMultipleWithDoc(string message, List<int> listCustomerId, int brokerId, ChatDocument chatDocument);

        Task<bool> SendMultipleAppLink(List<int> listCustomerId, int brokerId);

        Task<int> GetUnRead(int customerId, int brokerId = 0);

        Task<bool> SendMultipleVideoLink(string message, List<int> listCustomerId, int brokerId, string videoUrl, string VideoThumbnailData);

        Task<bool> SendMultipleAudioLink(string message, List<int> listCustomerId, int brokerId, string audioUrl);
    }
}
