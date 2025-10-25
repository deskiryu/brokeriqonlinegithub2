namespace BrokerIQ.Online.Models
{
    public class ConsentDocumentModel
    {
        public Dto.Enum.ConsentDocumentsEnum ConsentType { get; set; }
        public string Description { get; set; }
        public bool IsUrlConsent { get; set; }
        public string Url { get; set; }
        public int MajorVersion { get; set; } = 1;
        public int MinorVersion { get; set; } = 1;
        public bool YaviaRequired { get; set; }
    }
}
