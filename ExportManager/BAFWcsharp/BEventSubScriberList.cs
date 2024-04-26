using System;
using System.Collections.Generic;
using System.Linq;

namespace BAFW
{
    public class BEventSubScriberList
    {
        private Type _evType;
        private uint _evId = 0;
        private List<BActive> _objects;

        /*
         * ******************************************************************
         * \brief The event type for this subscriber list
         */
        public Type evType { get { return _evType; } }

        /*
         * ******************************************************************
         * \brief The event ID for this subscriber list
         */
        public uint Id { get { return _evId; } }

        /*
         * ******************************************************************
         * \brief 
         */
        public bool Add(BActive active)
        {
            if (_objects.Contains(active))
            {
                return false;
            }
            _objects.Add(active);
            return true;
        }

        /*
         * ******************************************************************
         * \brief 
         */
        public bool Remove(BActive active)
        {
            return _objects.Remove(active);
        }

        /*
         * ******************************************************************
         * \brief 
         */
        public BEventSubScriberList(BPublicEvent ev)
        {
            _evType = ev.GetType();
            _evId = ev.Id;
            _objects = new List<BActive>();
        }

        /*
         * ******************************************************************
         * \brief 
         */
        ~BEventSubScriberList()
        {
            _objects.Clear();
        }

        /*
         * ******************************************************************
         * \brief Post the given event to all subscribing active objects
         */
        internal void Publish(BPublicEvent ev)
        {
            foreach (var ao in _objects)
            {
                ao.PostEvent(ev);
            }
        }

        /*
         * ******************************************************************
         * \brief Determine if the active object is contained in this list
         */
        internal bool Contains(BActive active)
        {
            return _objects.Contains(active);
        }

    }
}
