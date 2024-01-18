using BrokerIQ.Dto.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace BrokerIQ.Online.Server.Models
{
    public class Video
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string GuidId { get; set; }

        public string Url { get; set; }

        public DateTime UploadDate { get; set; }

        public int BrokerId { get; set; }

        public bool AvailableToAll { get; set; }

        public bool Vetted { get; set; }

        public string VideoThumbnailData { get; set; }

        public VideoSendEnum VideoSendTypeId { get; set; }

        public DateTime? SendDate { get; set; }
        public string MessageContent { get; set; }
        public string BrokerName { get; set; }

        public bool BirthdayVideo { get => VideoSendTypeId == VideoSendEnum.BirthdayVideo; }

        public bool WelcomeVideo { get => VideoSendTypeId == VideoSendEnum.WelcomeVideo; }

        public bool MortgageVideo { get => VideoSendTypeId == VideoSendEnum.MortgageVideo; }
        public bool InsuranceVideo { get => VideoSendTypeId == VideoSendEnum.InsuranceVideo; }
        public bool SendDateVideo { get => VideoSendTypeId == VideoSendEnum.SendOnDate; }

        public bool SpecialVideo { get => BirthdayVideo || InsuranceVideo || MortgageVideo || InsuranceVideo; }

        public bool SendDateTick { get => VideoSendTypeId == VideoSendEnum.SendOnDate; }

        public string Identifier { get; set; }

        public bool NotVetted { get => Vetted==false; }

    }
}