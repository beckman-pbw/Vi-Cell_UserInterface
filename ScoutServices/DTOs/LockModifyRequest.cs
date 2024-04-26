using ScoutServices.Enums;

namespace ScoutServices.DTOs
{
    public class LockModifyRequest
    {
        public string Username { get; set; }
        public LockRequest Type { get; set; }
    }
}
