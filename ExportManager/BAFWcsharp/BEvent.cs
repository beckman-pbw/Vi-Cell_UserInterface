using System;

namespace BAFW
{
    public class BEvent
    {
        public enum EvType : UInt32
        {
            None = 0,
            Entry,
            Exit,
            Init,
            Public,
            Private,
            Timer
        }

        public EvType MyType { get; private set; } = EvType.None;

        public UInt32 Id { get; private set; } = 0;

        public UInt32 AppData { get; protected set; }

        internal void SetAppData(UInt32 appData)
        {
            AppData = appData;
        }

        public bool AppBool { get { return (AppData != 0); } }

        protected internal BEvent(EvType typ, UInt32 evId, UInt32 appData = 0)
        {
            MyType = typ;
            Id = evId;
            AppData = appData;
        }

        ~BEvent() { }

        public bool CheckEventId(uint mask)
        {
	        return (Id & mask) == mask;
        }
    }

	//*************************************************************************
	public class BTimerEvent : BEvent
    {
        public BTimerEvent(UInt32 evId)
            : base(EvType.Timer, evId)
        {
        }
    }

	//*************************************************************************
    public class BPublicEvent : BEvent
    {
        public BPublicEvent(UInt32 evId, UInt32 appData = 0)
            : base(EvType.Public, evId, appData)
        {
        }

        public BPublicEvent(UInt32 evId, bool appBool)
            : base(EvType.Public, evId, (UInt32)(appBool ? 1 : 0))
        {
        }

        ~BPublicEvent()
        {
        }
    }
    
	//*************************************************************************
    public class BPrivateEvent : BEvent
    {
        public UInt32 OrthoId { get; private set; } = 0;

        public BPrivateEvent(UInt32 evId, UInt32 appData = 0, UInt32 orthoId = 0)
            : base(EvType.Private, evId, appData)
        {
            OrthoId = orthoId;
        }

        ~BPrivateEvent()
        {
        }

    }
}