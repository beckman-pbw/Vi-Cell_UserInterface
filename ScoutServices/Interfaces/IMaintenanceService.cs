using ScoutUtilities.Enums;
using System;

namespace ScoutServices.Interfaces
{
    public interface IMaintenanceService
    {
	    IObservable<ePrimeReagentLinesState> SubscribePrimeReagentsStatus();
	    IObservable<ePurgeReagentLinesState> SubscribePurgeReagentsStatus();
	    IObservable<eFlushFlowCellState> SubscribeCleanFluidicsStatus();
	    IObservable<eDecontaminateFlowCellState> SubscribeDecontaminateStatus();
	    bool StartCleanFluidics();
	    bool PrimeReagents();
	    bool CancelPrimeReagents();
	    bool PurgeReagents();
	    bool CancelPurgeReagents();
	    bool Decontaminate();
	    bool CancelDecontaminate();
    }
}