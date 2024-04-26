using ScoutUtilities.Common;
using System.Collections.Generic;
using System.Linq;
using ScoutUtilities.Enums;

namespace ScoutUtilities.CustomEventArgs
{
    public class SequentialNamingEventArgs : BaseDialogEventArgs
    {
        public bool UseSequencing { get; set; }
        public bool TextItemIsFirst { get; set; }
        public List<SequentialNamingItem> SeqNamingItems { get; set; }
        
        public SequentialNamingEventArgs(bool useSequencing, List<SequentialNamingItem> items)
        {
            SizeToContent = true;
            UseSequencing = useSequencing;
            SeqNamingItems = items;
            TextItemIsFirst = SeqNamingItems.FirstOrDefault()?.SeqNamingType != SequentialNamingType.Integer;
        }
    }
}