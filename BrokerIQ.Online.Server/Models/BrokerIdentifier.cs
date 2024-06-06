using System.ComponentModel.DataAnnotations;

namespace BrokerIQ.Online.Server.Models
{
    public class BrokerIdentifier
    {
        public int Id { get; set; }

        public int BrokerId { get; set; }

        public bool IdentifierFound { get; set; }

        [Required]
        public string BundleIdentifier { get; set; }

        [Required]
        public string BackgroundColour { get; set; }

        [Required]
        public string TextColour { get; set; }

        [Required]
        [Url]
        public string HttpLink { get; set; }

        [Required]
        public string HttpAddress { get; set; }

        [Required]
        public string AppName { get; set; }

        public bool Invert { get; set; }

        [Required]
        public string LinkColour { get; set; }

        [Required]
        [Url]
        public string AppStoreLink { get; set; }

        [Required]
        [Url]
        public string PlayStoreLink { get; set; }

        [Required]
        public string FromEmailName { get; set; }

        [Required]
        [EmailAddress]
        public string FromEmailAddress { get; set; }

        [Required]
        [Url]
        public string WelcomeVideoUrl { get; set; }

        [Required]
        public string EmailTagLine { get; set; }

        [Required]
        public string HubClientConnectString { get; set; }

        [Required]
        public string HubClientName { get; set; }

        [Required]
        public string FirebaseKey { get; set; }

        [Required]
        public string FirebaseClient { get; set; }

        [Required]
        public bool InsuranceOnly { get; set; }

        public string HeaderColumnBGColour { get; set; }

        public string InvitationMessage { get; set; }

        public string OnboardingPage1Title { get; set; }

        public string OnboardingPage1SubTitle { get; set; }

        public string OnboardingPage2FirstText { get; set; }

        public string OnboardingPage2SecondText { get; set; }

        public string OnboardingPage3FirstText { get; set; }

        public string OnboardingPage3SecondText { get; set; }

        public string OnboardingPage3ThirdText { get; set; }

        public string OnboardingPage3FourthText { get; set; }

        public string OnboardingThankYouText { get; set; }

        public bool UseAIChat { get; set; }

        public bool IsLimitedBroker { get; set; }

        public bool HasVideo { get; set; }

        public int MaxVideos { get; set; }

        public int MaxTemplates { get; set; }

        public bool HasReminders { get; set; }

        public bool HasFilters { get; set; }

        public bool HasAudio { get; set; }

        public bool HasReferrals { get; set; }
    }
}
