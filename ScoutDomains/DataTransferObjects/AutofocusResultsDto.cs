using ScoutUtilities.Structs;
using System.Collections.Generic;

namespace ScoutDomains.DataTransferObjects
{
    public class AutofocusResultsDto
    {
        public bool IsFocusSuccessful { get; set; }
        public uint NumFocusDatapoints { get; set; }

        public List<AutofocusDatapoint> Dataset { get; set; }

        // Position identified as that of sharpest focus (motor position)
        public int BestAutofocusPosition { get; set; }

        // Configured offset (in microns) from sharpest focus
        public int BestFocusOffsetMicrons { get; set; }

        // Final position of autofocus after offset (motor position)
        public int FinalAutofocusPosition { get; set; }

        // Final image at the "bestfocus_af_position" position (motor position)
        public ImageDto BestAutofocusImage { get; set; }
    }
}
