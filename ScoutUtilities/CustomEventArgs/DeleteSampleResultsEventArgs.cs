using ScoutUtilities.Structs;
using System.Collections.Generic;

namespace ScoutUtilities.CustomEventArgs
{
    public class DeleteSampleResultsEventArgs : BaseDialogEventArgs
    {
        public List<uuidDLL> ItemsToDelete { get; set; }

        public DeleteSampleResultsEventArgs(List<uuidDLL> itemsToDelete)
        {
            ItemsToDelete = itemsToDelete;
            DialogIconPath = "/Images/Delete.png";
            DialogIconIsSquare = true;
        }
    }
}