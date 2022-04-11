using System;
using System.Collections.Generic;
using System.Text;

namespace BrokerIQ.Online.Server.Helper
{
    public class MessageCountState
    {
        private int _messageCount;
        public event Action OnMessageCountChange;
        public int MessageCount
        {
            get { return _messageCount; }
            set
            {
                _messageCount = value;
                NotifyMCStateChanged();
            }
        }

        private void NotifyMCStateChanged() => OnMessageCountChange?.Invoke();
    }
}
