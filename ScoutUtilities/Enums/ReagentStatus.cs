using System;

namespace ScoutUtilities.Enums
{
   
    public enum ReagentContainerStatus : UInt32
    {
        eOK = 0, // Reagent pack present, loaded, functional.
        eEmpty, // Reagent Pack present but with no remaining events
        eNotDetected, // Pack not detected
        eInvalid, // Pack not recognized / failed validation
        eExpired, // Pack expired (too long in-service or past shelf-life date)
        eFaulted, // Pack is in a faulted state (fluidic / mechanical / other)
        eUnloading, // Pack is being unloaded
        eUnloaded, // Pack is in an unloaded state (piercing mechanism retracted & idle) - may not be present.
        eLoading // Pack is being loaded (piercing mechanism activated / pack recognition and validation in progress)	
    }

  
    public enum ReagentContainerPosition : UInt16
    {
        eUnknown = 0xFFFF,
        eNone = 0,
        eMainBay_1,
        eDoorLeft_2,
        eDoorRight_3
    }

    public enum CellHealthFluidType
    {
	    Unknown = 0,
	    TrypanBlue = 1,
	    Cleaner = 2,
	    ConditioningSolution = 3,
	    Buffer = 4,
	    Diluent = 5
    }
}
