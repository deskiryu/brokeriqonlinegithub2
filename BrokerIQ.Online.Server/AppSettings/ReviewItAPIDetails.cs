namespace BrokerIQ.Online.AppSettings
{
    public class ReviewItAPIDetails
    {
        public string Version { get; set; }

        public string Url { get; set; }

        public string WS { get; set; }

        public int AutoLogoutTimeMs { get; set; }

        public string VideoConvertUrl { get; set; }

        public bool ShowProtection { get; set; }

        public bool ShowStaffType { get; set; }

        public bool HideErrorReloadFooter { get; set; }

        public bool ShowClientPictureColumn { get; set; }

        public bool ShowClientPhoneColumn { get; set; }

        public bool CanLoadInsuranceFromFile { get; set; }

        public bool CalendlyAvailable { get; set; }

        public bool ShowConnectToCustomer { get; set; }

        public bool IsYAHTheme { get; set; }
    }
}
