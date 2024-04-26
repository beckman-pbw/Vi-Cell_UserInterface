using System.Collections.Generic;

namespace ScoutDomains
{
    public class GenericParameters
    {
        public IList<GenericParametersDomain> GenericParametersList { get; set; }

        public GenericParameters()
        {
            GenericParametersList = new List<GenericParametersDomain>();
        }
    }
}

