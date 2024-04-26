using ScoutUtilities.Enums;
using ScoutUtilities.Interfaces;
using System.Collections.Generic;

namespace ScoutUtilities.CustomEventArgs
{
    public class AddSignatureEventArgs : BaseDialogEventArgs
    {
        public bool ReturnPassword { get; set; }
        public List<ISignature> AvailableSignatures { get; set; }
        
        public ISignature SignatureSelected { get; set; }
        public string SignaturePassword { get; set; }

        public AddSignatureEventArgs(List<ISignature> availableSigns, bool returnPassword = false)
        {
            AvailableSignatures = availableSigns;
            ReturnPassword = returnPassword;
            SizeToContent = true;
        }
    }
}