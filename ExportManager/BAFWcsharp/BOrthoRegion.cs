using System;

namespace BAFW
{
    public class BOrthoRegion : BHsm
    {

        #region member_variables
        #endregion

        public UInt32 OrthoId { get; private set; } = 0;

        /*
         * ******************************************************************
         * \brief
         */
        protected BActive Owner { get; private set; } = null;

        /*
         * ******************************************************************
         * \brief
         */
        public BOrthoRegion(BActive owner)
        {
            Owner = owner;            
        }

        /*
         * ******************************************************************
         * \brief
         */
        public BOrthoRegion(BActive owner, UInt32 orthoId)
        {
            Owner = owner;
            OrthoId = orthoId;
        }

        /*
         * ******************************************************************
         * \brief
         */
        protected bool PostPrivateEventToOwner(BPrivateEvent pEvent)
        {
            return (Owner.GetEventQueue().Put(pEvent, BEventQueue.WaitType.WaitForever) == BEventQueue.Status.Ok);
        }

        /*
         * ******************************************************************
         * \brief
         */
        protected bool PostInternalEvent(BPrivateEvent pEvent)
        {
            return Owner.PostInternalEvent(pEvent);
        }


    }

}
