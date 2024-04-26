using ScoutUtilities.Enums;
using System;

namespace ScoutUtilities.Common
{
    public class HubMessage
    {
        public HubMessage(DateTime time, MessageType type, string content)
        {
            Content = content;
            Time = time;
            Type = type;
            TimesShown = 0;
        }

        public int TimesShown { get; set; }

        public string Content { get; set; }

        public DateTime Time { get; set; }

        public MessageType Type { get; set; }

        public string DisplayTime => Misc.ConvertToCustomLongDateTimeFormat(Time);
    }
}
