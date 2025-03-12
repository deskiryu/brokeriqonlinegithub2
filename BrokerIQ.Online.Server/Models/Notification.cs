using System;
using System.Collections.Generic;
using System.IO;

namespace BrokerIQ.Online.Models
{
    public class Notification
    {
        public string Message { get; set; }
        public string VideoUrl { get; set; }
        public List<int> Targets { get; set; }
        public bool SentToAll { get; set; }
        public DateTime SentDate { get; set; }
        public int BrokerId { get; set; }
        public string AudioUrl { get; set; }
        public string VideoName => Path.GetFileNameWithoutExtension(VideoUrl);
        public string AudioName => Path.GetFileNameWithoutExtension(AudioUrl);
        public int? BrokerStaffId { get; set; }
        public string BrokerStaffName { get; set; }
        public string BrokerName { get; set; }
        public bool IsNotification { get => string.IsNullOrEmpty(VideoUrl) && string.IsNullOrEmpty(AudioUrl); }
        public bool IsVideo { get => !string.IsNullOrEmpty(VideoUrl); }
        public bool IsAudio { get => !string.IsNullOrEmpty(AudioUrl); }

        public string NotificationType { get => IsAudio ? "Audio" : IsVideo ? "Video" : "Chat"; }
    }
}
