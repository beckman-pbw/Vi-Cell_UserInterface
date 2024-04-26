using ScoutServices.Enums;
using ScoutUtilities.Enums;

namespace ScoutServices.DTOs
{
    public class ConfigSubjectDto
    {
        public HawkeyeError Result { get; set; }
        public ConfigState State { get; set; }
        public byte[] FileData { get; set; }
    }
}
