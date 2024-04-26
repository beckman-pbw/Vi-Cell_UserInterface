using System;

namespace ScoutDomains.Reagent
{
    public class ReagentInfoRecordDomain : ICloneable
    {
        public string PackNumber { get; set; }
       
        public int LotNumber { get; set; }
       
        public string ReagentName { get; set; }
       
        public DateTime ExpirationDate { get; set; }
       
        public DateTime InServiceDate { get; set; }
        
        public DateTime EffectiveExpirationDate { get; set; }

        public object Clone()
        {
            return (ReagentInfoRecordDomain)MemberwiseClone();
        }
    }
}
