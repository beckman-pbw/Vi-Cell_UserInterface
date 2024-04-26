using ApiProxies;
using ApiProxies.Commands.Reagent;
using ApiProxies.Misc;
using JetBrains.Annotations;
using ScoutDomains;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using ScoutModels.Common;

namespace ScoutModels
{
    public class ReagentModel : BaseDisposableNotifyPropertyChanged
    {
        #region Constructor        

        public ReagentModel()
        {
            ReagentContainers = new ObservableCollection<ReagentContainerStateDomain>();

            _subscribedToReagentUnloadStatus = false;
            _subscribedToReagentUnloadComplete = false;
            _subscribedToReagentLoadStatus = false;
            _subscribedToReagentLoadComplete = false;
            _subscribedToPrimeReagentLines = false;
            _subscribedToFlowCellFlushStatus = false;
            _subscribedToFlowCellDecontaminateStatus = false;
            _subscribedToCleanFluidicsStatus = false;
        }

		protected override void DisposeUnmanaged()
        {
            UnsubscribeFromFlowCellDecontaminateStatus();
            UnsubscribeFromFlowCellFlushStatus();
            UnsubscribeFromPrimeReagentLines();
            UnsubscribeFromReagentLoadComplete();
            UnsubscribeFromReagentLoadStatus();
            UnsubscribeFromReagentUnloadComplete();
            UnsubscribeFromReagentUnloadStatus();
            UnsubscribeFromCleanFluidicsStatus();
            base.DisposeUnmanaged();
        }

        #endregion

        #region Properties & Fields

        public event EventHandler<ApiEventArgs<ReagentLoadSequence>> LoadStatusChanged;
        public event EventHandler<ApiEventArgs<ReagentLoadSequence>> LoadCompleted;
        public event EventHandler<ApiEventArgs<ReagentUnloadSequence>> UnloadStatusChanged;
        public event EventHandler<ApiEventArgs<ReagentUnloadSequence>> UnloadCompleted;
        public event EventHandler<ApiEventArgs<ePrimeReagentLinesState>> PrimeReagentStateChanged;
        public event EventHandler<ApiEventArgs<eFlushFlowCellState>> FlowCellFlushStateChanged;
        public event EventHandler<ApiEventArgs<eDecontaminateFlowCellState>> FlowCellDecontaminateStateChanged;
        public event EventHandler<ApiEventArgs<eFlushFlowCellState>> CleanFluidicsStateChanged;
        public event EventHandler<ApiEventArgs<ePurgeReagentLinesState>> PurgeReagentStateChanged;

		private bool _subscribedToReagentUnloadStatus;
        private bool _subscribedToReagentUnloadComplete;
        private bool _subscribedToReagentLoadStatus;
        private bool _subscribedToReagentLoadComplete;
        private bool _subscribedToPrimeReagentLines;
        private bool _subscribedToFlowCellFlushStatus;
        private bool _subscribedToFlowCellDecontaminateStatus;
        private bool _subscribedToCleanFluidicsStatus;
        private bool _subscribedToPurgeReagentLines;


		public string PartNumber
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string EventsRemaining
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string ProgressIndicator
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string ReagentContainerStatusAsStr
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<ReagentContainerStateDomain> ReagentContainers
        {
            get { return GetProperty<ObservableCollection<ReagentContainerStateDomain>>(); }
            set
            {
                SetProperty(value);
                if (value != null && value.Any()) PartNumber = value[0]?.PartNumber ?? string.Empty;
                else PartNumber = string.Empty;
            }
        }

        public ReagentContainerStatus ReagentHealth
        {
            get { return GetProperty<ReagentContainerStatus>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Event Handlers

        private void HandleUnloadStatusChanged(object sender, ApiEventArgs<ReagentUnloadSequence> e)
        {
            UnloadStatusChanged?.Invoke(this, e);
        }

        private void HandleReagentLinesPrimeStateChanged(object sender, ApiEventArgs<ePrimeReagentLinesState> e)
        {
	        PrimeReagentStateChanged?.Invoke(this, e);

            if (e.Arg1 == ePrimeReagentLinesState.prl_Completed || e.Arg1 == ePrimeReagentLinesState.prl_Failed)
            {
                UnsubscribeFromPrimeReagentLines();
            }
        }

        private void HandleFlowCellFlushStateChanged(object sender, ApiEventArgs<eFlushFlowCellState> e)
        {
            FlowCellFlushStateChanged?.Invoke(this, e);

            if (e.Arg1 == eFlushFlowCellState.ffc_Completed || e.Arg1 == eFlushFlowCellState.ffc_Failed)
            {
                UnsubscribeFromFlowCellFlushStatus();
            }
        }

        private void HandleFlowCellDecontaminateStateChanged(object sender, ApiEventArgs<eDecontaminateFlowCellState> e)
        {
            FlowCellDecontaminateStateChanged?.Invoke(this, e);

            if (e.Arg1 == eDecontaminateFlowCellState.dfc_Completed || e.Arg1 == eDecontaminateFlowCellState.dfc_Failed)
            {
                UnsubscribeFromFlowCellDecontaminateStatus();
            }
        }

        private void HandleCleanFluidicsStateChanged(object sender, ApiEventArgs<eFlushFlowCellState> e)
        {
	        CleanFluidicsStateChanged?.Invoke(this, e);

	        if (e.Arg1 == eFlushFlowCellState.ffc_Completed || e.Arg1 == eFlushFlowCellState.ffc_Failed)
	        {
				UnsubscribeFromCleanFluidicsStatus();
	        }
        }

        private void HandleReagentLinesPurged(object sender, ApiEventArgs<ePurgeReagentLinesState> e)
        {
	        PurgeReagentStateChanged?.Invoke(this, e);

	        if (e.Arg1 == ePurgeReagentLinesState.dprl_Completed || e.Arg1 == ePurgeReagentLinesState.dprl_Failed)
	        {
		        UnsubscribeFromPurgeReagentLines();
	        }
        }

		private void HandleLoadCompleted(object sender, ApiEventArgs<ReagentLoadSequence> e)
        {
            LoadCompleted?.Invoke(this, e);
            // Is unsubscribe appropriate?
            LoadCompleted?.Invoke(this, e);
            switch (e.Arg1)
            {
                case ReagentLoadSequence.eLComplete:
                case ReagentLoadSequence.eLFailure_DoorLatchTimeout:
                case ReagentLoadSequence.eLFailure_ReagentSensorDetect:
                case ReagentLoadSequence.eLFailure_NoReagentsDetected:
                case ReagentLoadSequence.eLFailure_NoWasteDetected:
                case ReagentLoadSequence.eLFailure_ReagentInvalid:
                case ReagentLoadSequence.eLFailure_ReagentEmpty:
                case ReagentLoadSequence.eLFailure_ReagentExpired:
                case ReagentLoadSequence.eLFailure_InvalidContainerLocations:
                case ReagentLoadSequence.eLFailure_ProbeInsert:
                case ReagentLoadSequence.eLFailure_Fluidics:
                case ReagentLoadSequence.eLFailure_StateMachineTimeout:
                    SubscribeToReagentLoad(false);
                    break;
                default:
                    break;
            }
        }

        private void HandleLoadStatusChanged(object sender, ApiEventArgs<ReagentLoadSequence> e)
        {
            LoadStatusChanged?.Invoke(this, e);
        }

        private void HandleUnloadCompleted(object sender, ApiEventArgs<ReagentUnloadSequence> e)
        {
            UnloadCompleted?.Invoke(this, e);
            // Is unsubscribe appropriate?
            switch (e.Arg1)
            {
                case ReagentUnloadSequence.eULComplete:
                case ReagentUnloadSequence.eULFailed_DrainPurge:
                case ReagentUnloadSequence.eULFailed_ProbeRetract:
                case ReagentUnloadSequence.eULFailed_DoorUnlatch:
                case ReagentUnloadSequence.eULFailure_StateMachineTimeout:
                    UnsubscribeFromReagentUnloadStatus();
                    UnsubscribeFromReagentUnloadComplete();
                    break;
            }
        }

        #endregion

        #region Private Methods

        #region Subscription Tracker Methods

        private void SubscribeToReagentUnloadStatus()
        {
            if (!_subscribedToReagentUnloadStatus)
            {
                ApiEventBroker.Instance.Subscribe<ReagentUnloadSequence>(ApiEventType.Reagent_Unload_Status, HandleUnloadStatusChanged);
                _subscribedToReagentUnloadStatus = true;
            }
        }

        private void SubscribeToReagentUnloadComplete()
        {
            if (!_subscribedToReagentUnloadComplete)
            {
                ApiEventBroker.Instance.Subscribe<ReagentUnloadSequence>(ApiEventType.Reagent_Unload_Complete, HandleUnloadCompleted);
                _subscribedToReagentUnloadComplete = true;
            }
        }

        private void SubscribeToReagentLoadStatus()
        {
            if (!_subscribedToReagentLoadStatus)
            {
                ApiEventBroker.Instance.Subscribe<ReagentLoadSequence>(ApiEventType.Reagent_Load_Status, HandleLoadStatusChanged);
                _subscribedToReagentLoadStatus = true;
            }
        }

        private void SubscribeToReagentLoadComplete()
        {
            if (!_subscribedToReagentLoadComplete)
            {
                ApiEventBroker.Instance.Subscribe<ReagentLoadSequence>(ApiEventType.Reagent_Load_Complete, HandleLoadCompleted);
                _subscribedToReagentLoadComplete = true;
            }
        }

        private void SubscribeToPrimeReagentLines()
        {
            if (!_subscribedToPrimeReagentLines)
            {
                ApiEventBroker.Instance.Subscribe<ePrimeReagentLinesState>(ApiEventType.Prime_Reagent_Lines, HandleReagentLinesPrimeStateChanged);
                _subscribedToPrimeReagentLines = true;
            }
        }

        private void SubscribeToFlowCellFlushStatus()
        {
            if (!_subscribedToFlowCellFlushStatus)
            {
                ApiEventBroker.Instance.Subscribe<eFlushFlowCellState>(ApiEventType.Flowcell_Flush_Status, HandleFlowCellFlushStateChanged);
                _subscribedToFlowCellFlushStatus = true;
            }
        }

        private void SubscribeToCleanFluidics()
        {
	        if (!_subscribedToCleanFluidicsStatus)
	        {
		        ApiEventBroker.Instance.Subscribe<eFlushFlowCellState>(ApiEventType.Flowcell_Flush_Status, HandleCleanFluidicsStateChanged);
		        _subscribedToCleanFluidicsStatus = true;
	        }
        }

        private void SubscribeToPurgeReagentLines()
        {
	        if (!_subscribedToPurgeReagentLines)
	        {
		        ApiEventBroker.Instance.Subscribe<ePurgeReagentLinesState>(ApiEventType.Purge_Reagent_Lines, HandleReagentLinesPurged);
		        _subscribedToPurgeReagentLines = true;
	        }
        }

        private void SubscribeToFlowCellDecontaminateStatus()
        {
            if (!_subscribedToFlowCellDecontaminateStatus)
            {
                ApiEventBroker.Instance.Subscribe<eDecontaminateFlowCellState>(ApiEventType.Flowcell_Decontaminate_Status, HandleFlowCellDecontaminateStateChanged);
                _subscribedToFlowCellDecontaminateStatus = true;
            }
        }

        private void UnsubscribeFromReagentUnloadStatus()
        {
            if (_subscribedToReagentUnloadStatus)
            {
                ApiEventBroker.Instance.Unsubscribe<ReagentUnloadSequence>(ApiEventType.Reagent_Unload_Status, HandleUnloadStatusChanged);
                _subscribedToReagentUnloadStatus = false;
            }
        }

        private void UnsubscribeFromReagentUnloadComplete()
        {
            if (_subscribedToReagentUnloadComplete)
            {
                ApiEventBroker.Instance.Unsubscribe<ReagentUnloadSequence>(ApiEventType.Reagent_Unload_Complete, HandleUnloadCompleted);
                _subscribedToReagentUnloadComplete = false;
            }
        }

        private void UnsubscribeFromReagentLoadStatus()
        {
            if (_subscribedToReagentLoadStatus)
            {
                ApiEventBroker.Instance.Unsubscribe<ReagentLoadSequence>(ApiEventType.Reagent_Load_Status, HandleLoadStatusChanged);
                _subscribedToReagentLoadStatus = false;
            }
        }

        private void UnsubscribeFromReagentLoadComplete()
        {
            if (_subscribedToReagentLoadComplete)
            {
                ApiEventBroker.Instance.Unsubscribe<ReagentLoadSequence>(ApiEventType.Reagent_Load_Complete, HandleLoadCompleted);
                _subscribedToReagentLoadComplete = false;
            }
        }

        private void UnsubscribeFromPrimeReagentLines()
        {
            if (_subscribedToPrimeReagentLines)
            {
                ApiEventBroker.Instance.Unsubscribe<ePrimeReagentLinesState>(ApiEventType.Prime_Reagent_Lines, HandleReagentLinesPrimeStateChanged);
                _subscribedToPrimeReagentLines = false;
            }
        }

        private void UnsubscribeFromFlowCellFlushStatus()
        {
            if (_subscribedToFlowCellFlushStatus)
            {
                ApiEventBroker.Instance.Unsubscribe<eFlushFlowCellState>(ApiEventType.Flowcell_Flush_Status, HandleFlowCellFlushStateChanged);
                _subscribedToFlowCellFlushStatus = false;
            }
        }

        private void UnsubscribeFromCleanFluidicsStatus()
        {
	        if (_subscribedToCleanFluidicsStatus)
	        {
		        ApiEventBroker.Instance.Unsubscribe<eFlushFlowCellState>(ApiEventType.Flowcell_Flush_Status, HandleCleanFluidicsStateChanged);
		        _subscribedToCleanFluidicsStatus = false;
	        }
        }

        private void UnsubscribeFromPurgeReagentLines()
        {
	        if (_subscribedToPurgeReagentLines)
	        {
		        ApiEventBroker.Instance.Unsubscribe<ePurgeReagentLinesState>(ApiEventType.Flowcell_Flush_Status, HandleReagentLinesPurged);
		        _subscribedToPurgeReagentLines = false;
	        }
        }

		private void UnsubscribeFromFlowCellDecontaminateStatus()
        {
            if (_subscribedToFlowCellDecontaminateStatus)
            {
                ApiEventBroker.Instance.Unsubscribe<eDecontaminateFlowCellState>(ApiEventType.Flowcell_Decontaminate_Status, HandleFlowCellDecontaminateStateChanged);
                _subscribedToFlowCellDecontaminateStatus = false;
            }
        }

        #endregion

        private void GetReagentStatus(ReagentContainerStateDomain reagentContainerStatus)
        {
            switch (reagentContainerStatus.Status)
            {
                case ReagentContainerStatus.eOK:
                    ValidateUsesRemaining(Convert.ToInt32(reagentContainerStatus.EventsRemaining));
                    break;
                case ReagentContainerStatus.eEmpty:
                    ValidateUsesRemaining(Convert.ToInt32(reagentContainerStatus.EventsRemaining));
                    break;
            }
        }

        private void ValidateUsesRemaining(int eventRemaining)
        {
            if (eventRemaining >= 30)
                ReagentContainerStatusAsStr = string.Empty;
            if (eventRemaining < 30 && eventRemaining >= 10)
                ReagentContainerStatusAsStr = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_LOW");
            if (eventRemaining < 10)
                ReagentContainerStatusAsStr = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_WARNING");
        }

        private void SubscribeToReagentUnload(bool subscribe)
        {
            if (subscribe)
            {
                SubscribeToReagentUnloadStatus();
                SubscribeToReagentUnloadComplete();
            }
            else
            {
                UnsubscribeFromReagentUnloadStatus();
                UnsubscribeFromReagentUnloadComplete();
            }
        }

        private void SubscribeToReagentLoad(bool subscribe)
        {
            if (subscribe)
            {
                SubscribeToReagentLoadStatus();
                SubscribeToReagentLoadComplete();
            }
            else
            {
                UnsubscribeFromReagentLoadStatus();
                UnsubscribeFromReagentLoadComplete();
            }
        }

        private static List<ReagentContainerStateDomain> CreateReagentStatusList(List<ReagentContainerState> reagentContainerStateAll, IntPtr reagentPtr, string container)
        {
            var containers = new List<ReagentContainerStateDomain>();
            if (reagentContainerStateAll != null && reagentContainerStateAll.Any())
            {
                var reagentDefinitionList = GetReagentDefinition();
                foreach (var reagentContainer in reagentContainerStateAll)
                {
                    var dicReagentState = CrateReagentStateDomain(reagentDefinitionList, reagentContainer);
#if DEBUG
                    LogReagentContainerState(reagentContainer);
#endif
                    containers.Add(new ReagentContainerStateDomain()
                    {
                        ContainerName = container + " " + (Convert.ToInt16(reagentContainer.position) + 1),
                        EventsPossible =
                                ((ReagentState)(Marshal.PtrToStructure(reagentContainer.reagent_states,
                                    typeof(ReagentState)))).events_possible,
                        EventsRemaining = reagentContainer.events_remaining,
                        ExpiryDate = DateTimeConversionHelper.FromDaysUnixToDateTime(reagentContainer.exp_date),
                        LotInformation = reagentContainer.lot_information.ToSystemString(),
                        NumberOfReagents = reagentContainer.num_reagents,
                        PartNumber = reagentContainer.bci_part_number.ToSystemString(),
                        ReagentNames = dicReagentState,
                        Status = reagentContainer.status
                    }
                    );
                }

                HawkeyeCoreAPI.Reagent.FreeReagentStateALLAPI(reagentPtr);
            }

            return containers;
        }

        private static Dictionary<string, IList<ReagentStateDomain>> CrateReagentStateDomain(List<ReagentDefinition> reagentDefinitionList, ReagentContainerState reagentContainerState)
        {
            var dicReagentState = new Dictionary<string, IList<ReagentStateDomain>>();
            var reagentPtr = reagentContainerState.reagent_states;
            for (int i = 0; i < reagentContainerState.num_reagents; i++)
            {
                var reagentState = (ReagentState)(Marshal.PtrToStructure(reagentPtr, typeof(ReagentState)));
                var size = Marshal.SizeOf(reagentState);
                foreach (var r in reagentDefinitionList)
                {
                    if (r.reagent_index == reagentState.reagent_index)
                    {
                        var reagent = new ReagentStateDomain()
                        {
                            DaysToExpiration = (int)reagentContainerState.exp_date,
                            MixingCycles = r.mixing_cycles,
                            ReagentIndex = r.reagent_index,
                            ReagentName = r.label,
                        };
                        reagent.EventsPossible = reagentState.events_possible;
                        reagent.EventsRemaining = reagentState.events_remaining;
                        reagent.LotInformation = reagentState.lot_information.ToSystemString();
                        reagent.ValveLocation = reagentState.valve_location;
                        reagent.PartNumber = reagentState.reagent_index.ToString();
                        dicReagentState.Add(reagent.ReagentName, new List<ReagentStateDomain> { reagent });
                        break;
                    }
                }
                reagentPtr += size;
            }

            return dicReagentState;
        }

        private static List<ReagentDefinition> GetReagentDefinition()
        {
            var reagentDefinition = new List<ReagentDefinition>();
            uint numDefinitions = 0;
            var hawkeyeError = HawkeyeCoreAPI.Reagent.GetReagentDefinitionsAPI(ref numDefinitions, ref reagentDefinition);
            Log.Info("GetReagentDefinition: " + numDefinitions);
            Misc.LogOnHawkeyeError("GetReagentDefinition", hawkeyeError);
#if DEBUG
			foreach (var def in reagentDefinition)
            {
                LogReagentDefinition(def);
            }
#endif
            return reagentDefinition;
        }

        private static void LogReagentContainerState(ReagentContainerState reagentStatus)
        {
	        Log.Info(
		        "\nReagentContainerState:: " +
	                 "\n	status: " + reagentStatus.status.ToString() +
	                 "\n	events_remaining: " + reagentStatus.events_remaining +
	                 "\n	in_service_date: " + reagentStatus.in_service_date +
	                 "\n	exp_date: " + reagentStatus.exp_date +
	                 "\n	lot_information: " + reagentStatus.lot_information +
	                 "\n	num_reagents: " + reagentStatus.num_reagents +
	                 "\n	position: " + reagentStatus.position.ToString() +
	                 "\n	bci_part_number: " + reagentStatus.bci_part_number +
	                 "\n	reagentStates : " + reagentStatus.reagent_states +
	                 "\n	events_remaining : " + reagentStatus.events_remaining +
	                 "\n	num_reagents : " + reagentStatus.num_reagents);
        }

        private static void LogReagentDefinition(ReagentDefinition reagentDef)
        {
            Log.Info($"reagent_index: {reagentDef.reagent_index} , label: {reagentDef.label}, mixing_cycles: {reagentDef.mixing_cycles}");
        }

        private void UpdateReagentContainers(ref List<ReagentContainerStateDomain> container)
        {
            /* This function updates each individual variable within ReagentContainers, rather than just
             * setting ReagentContainers = container, because the dropdowns within the Reagent dialog
             * will automatically collapse each time this function is called (~once per second) if
             * not done this way.
             */ 
            for (int i = 0; i < container.Count; i++)
            {
                if (ReagentContainers.Count > i)
                {
                    ReagentContainers[i].Status = container[i].Status;
                    ReagentContainers[i].EventsRemaining = container[i].EventsRemaining;
                    ReagentContainers[i].EventsPossible = container[i].EventsPossible;
                    ReagentContainers[i].ContainerName = container[i].ContainerName;
                    ReagentContainers[i].PartNumber = container[i].PartNumber;
                    ReagentContainers[i].ExpiryDate = container[i].ExpiryDate;
                    ReagentContainers[i].LotInformation = container[i].LotInformation;
                    ReagentContainers[i].NumberOfReagents = container[i].NumberOfReagents;

                    // Reset ReagentContainers if anything is null or is empty to update GUI dialog
                    if ((container[i].ReagentNames == null || ReagentContainers[i].ReagentNames == null) ||
                        (container[i].ReagentNames.Count <= 0 || ReagentContainers[i].ReagentNames.Count <= 0))
                    {
                        ReagentContainers[i].ReagentNames = container[i].ReagentNames;
                    }
                    else
                    {
                        foreach (var key in container[i].ReagentNames.Keys)
                        {
                            if (!ReagentContainers[i].ReagentNames.ContainsKey(key))
                            {
                                ReagentContainers[i].ReagentNames.Add(key, container[i].ReagentNames[key]);
                            }
                            else
                            {
                                ReagentContainers[i].ReagentNames[key][0].EventsPossible = container[i].ReagentNames[key][0].EventsPossible;
                                ReagentContainers[i].ReagentNames[key][0].EventsRemaining = container[i].ReagentNames[key][0].EventsRemaining;
                                ReagentContainers[i].ReagentNames[key][0].PartNumber = container[i].ReagentNames[key][0].PartNumber;
                                ReagentContainers[i].ReagentNames[key][0].LotInformation = container[i].ReagentNames[key][0].LotInformation;
                                ReagentContainers[i].ReagentNames[key][0].DaysToExpiration = container[i].ReagentNames[key][0].DaysToExpiration;
                                ReagentContainers[i].ReagentNames[key][0].MixingCycles = container[i].ReagentNames[key][0].MixingCycles;
                                ReagentContainers[i].ReagentNames[key][0].ReagentIndex = container[i].ReagentNames[key][0].ReagentIndex;
                                ReagentContainers[i].ReagentNames[key][0].ReagentName = container[i].ReagentNames[key][0].ReagentName;
                                ReagentContainers[i].ReagentNames[key][0].ValveLocation = container[i].ReagentNames[key][0].ValveLocation;
                            }
                        }
                    }
                }
            }
        }

        #endregion

	#region Public Methods

        public static List<ReagentContainerStateDomain> GetReagentContainerStatusAll()
        {
            var container = ScoutLanguageResources.LanguageResourceHelper.Get("LID_ResultHeader_Container");

            var reagentContainerStateAll = new List<ReagentContainerState>();
            int numReagent = 0;
            var hawkeyeError = HawkeyeCoreAPI.Reagent.GetReagentContainerStatusAllAPI(ref reagentContainerStateAll, (byte)numReagent);
            Log.Info($"GetReagentContainerStatusAll: # ReagentContainers: {reagentContainerStateAll.Count}");
            Misc.LogOnHawkeyeError($"GetReagentContainerStatusAll", hawkeyeError.Item1);
            var reagentList = CreateReagentStatusList(reagentContainerStateAll, hawkeyeError.Item2, container);

            if (!hawkeyeError.Item1.Equals(HawkeyeError.eSuccess))
            {
                reagentList.Add(new ReagentContainerStateDomain() { Status = ReagentContainerStatus.eNotDetected, ReagentNames = new Dictionary<string, IList<ReagentStateDomain>>() });
            }

            return reagentList;
        }

        public void OnRefreshReagent()
        {
            var container = GetReagentContainerStatusAll();
            if (container.Count > 0)
            {
                if (ReagentContainers.Count <= 0)
                    ReagentContainers = new ObservableCollection<ReagentContainerStateDomain>(container);
                else
                {
                    UpdateReagentContainers(ref container);
                }
                ReagentHealth = ReagentContainers[0].Status;
                EventsRemaining = Misc.ConvertToString(ReagentContainers[0].EventsRemaining);
                ProgressIndicator = Misc.ConvertToString((ReagentContainers[0].EventsRemaining * 100) / ReagentContainers[0].EventsPossible);
                GetReagentStatus(ReagentContainers[0]);
            }
            else
            {
                ReagentContainers.Add(new ReagentContainerStateDomain());
            }
        }

        [MustUseReturnValue("Use HawkeyeError")] 
        public HawkeyeError UnloadReagentPack(ReagentUnloadOption unloadOption)
        {
            SubscribeToReagentUnload(true);
            var hawkeyeError = new UnloadReagentPack(unloadOption).Invoke();
            if (hawkeyeError != HawkeyeError.eSuccess)
            {
                SubscribeToReagentUnload(false);
            }

            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")] 
        public HawkeyeError LoadReagentPack()
        {
            SubscribeToReagentLoad(true);
            var hawkeyeError = new LoadReagentPack().Invoke();
            if (hawkeyeError != HawkeyeError.eSuccess)
            {
                SubscribeToReagentLoad(false);
            }

            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError SampleTubeDiscardTrayEmptied()
        {
            Log.Debug($"{nameof(SampleTubeDiscardTrayEmptied)}: <enter>");
            return HawkeyeCoreAPI.Reagent.SampleTubeDiscardTrayEmptiedAPI();
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError CancelFlushFlowCell()
        {
            var hawkeyeError = HawkeyeCoreAPI.Reagent.CancelFlushFlowCellAPI();
            Log.Debug($"{nameof(CancelFlushFlowCell)}:: hawkeyeError:" + hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public virtual HawkeyeError CancelDecontaminateFlowCell()
        {
            var hawkeyeError = HawkeyeCoreAPI.Reagent.CancelDecontaminateFlowCellAPI();
            Log.Debug($"{nameof(CancelDecontaminateFlowCell)}:: hawkeyeError:" + hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError CancelPrimeReagentLines()
        {
	        var hawkeyeError = HawkeyeCoreAPI.Reagent.CancelPrimeReagentLinesAPI();
	        Log.Debug($"{nameof(CancelPrimeReagentLines)}:: hawkeyeError:" + hawkeyeError);
	        return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError CancelPurgeReagentLines()
        {
	        var hawkeyeError = HawkeyeCoreAPI.Reagent.CancelPurgeReagentLinesAPI();
	        Log.Debug($"{nameof(CancelPurgeReagentLines)}:: hawkeyeError:" + hawkeyeError);
	        return hawkeyeError;
        }

		[MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError StartDecontaminateFlowCell()
        {
	        try
	        {
		        SubscribeToFlowCellDecontaminateStatus();
		        var apiCommand = new StartDecontaminateFlowCell();
		        return apiCommand.Invoke();
	        }
	        catch (Exception)
	        {
		        UnsubscribeFromFlowCellDecontaminateStatus();
		        throw;
	        }
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError StartFlushFlowCell()
        {
	        try
	        {
		        SubscribeToFlowCellFlushStatus();
		        var apiCommand = new StartFlushFlowCell();
		        return apiCommand.Invoke();
	        }
	        catch (Exception)
	        {
		        UnsubscribeFromFlowCellFlushStatus();
		        throw;
	        }
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError StartPrimeReagentLines()
        {
	        try
	        {
		        SubscribeToPrimeReagentLines();
		        var apiCommand = new StartPrimeReagentLines();
		        return apiCommand.Invoke();
	        }
			catch (Exception)
	        {
		        UnsubscribeFromPrimeReagentLines();
		        throw;
	        }
        }

		[MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError StartCleanFluidics()
        {
	        try
	        {
		        SubscribeToCleanFluidics();
		        var apiCommand = new StartCleanFluidics();
		        return apiCommand.Invoke();
	        }
	        catch (Exception)
	        {
		        UnsubscribeFromFlowCellFlushStatus();
		        throw;
	        }
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError StartPurgeReagentLines()
        {
	        try
	        {
		        SubscribeToPurgeReagentLines();
		        var apiCommand = new StartPurgeReagentLines();
		        return apiCommand.Invoke();
	        }
	        catch (Exception)
	        {
		        UnsubscribeFromPurgeReagentLines();
		        throw;
	        }
        }

	#endregion

	}
}