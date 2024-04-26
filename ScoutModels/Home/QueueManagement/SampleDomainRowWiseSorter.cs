using log4net;
using ScoutDomains.Common;
using System.Collections;

namespace ScoutModels.Home.QueueManagement
{
    public class SampleDomainRowWiseSorter : IComparer
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public int Compare(object x, object y)
        {
            var obj1 = x as SampleDomain;
            var obj2 = y as SampleDomain;

            if (obj1 == null || obj2 == null)
            {
                Log.Debug("SampleDomainRowWiseSorter can only sort SampleDomain objects.");
                return -1;
            }

            if (obj1.Row == obj2.Row && obj1.Column == obj2.Column)
            {
                return 0;
            }
            if (obj1.Row == obj2.Row)
            {
                if (obj1.Column > obj2.Column)
                {
                    return 1;
                }
            }
            if (obj1.Row > obj2.Row)
            {
                return 1;
            }
            return -1;
        }
    }
}
