using System;
using System.Collections.Generic;
using System.Text;

namespace BrokerIQ.Online.Server.Helper
{
    public class LoggedInAppState
    {
        private bool _loggedIn;
        public event Action OnChange;
        public bool LoggedIn
        {
            get { return _loggedIn; }
            set
            {
                if (_loggedIn != value)
                {
                    _loggedIn = value;
                    NotifyStateChanged();
                }
            }
        }

        public bool IsAdmin { get; set; }

        public bool IsBroker { get; set; }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
