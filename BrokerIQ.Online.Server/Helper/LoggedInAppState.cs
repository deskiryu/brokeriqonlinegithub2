using System;

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

                    if (!_loggedIn)
                    {
                        IsAdmin = false;
                        IsBroker = false;
                        IsMinorAdmin = false;
                        FullName = string.Empty;
                    }

                    NotifyStateChanged();
                }
            }
        }

        public bool IsAdmin { get; set; }

        public bool IsBroker { get; set; }

        public bool IsMinorAdmin { get; set; }

        public string FullName { get; set; }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
