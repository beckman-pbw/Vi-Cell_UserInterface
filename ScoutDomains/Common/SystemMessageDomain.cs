using ScoutUtilities.Enums;

namespace ScoutDomains
{
    public class SystemMessageDomain
    {
        public string Message { get; set; }
        public MessageType MessageType { get; set; }
        public bool IsMessageActive { get; set; }
    }
}