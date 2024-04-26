using ApiProxies.Misc;
using Ninject.Extensions.Logging;
using ScoutServices.Interfaces;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using System;
using System.Reactive.Subjects;
using ScoutModels;
using ScoutModels.Service;

namespace ScoutServices
{
	public class MaintenanceService : Disposable, IMaintenanceService
	{

	#region Properties & Fields

		private readonly ILogger _log;
		private readonly DustReferenceModel _dustReferenceModel;
		private readonly ReagentModel _reagentModel;
		private static readonly Subject<ePrimeReagentLinesState> _primeReagentsStatusCallbackSubject = new Subject<ePrimeReagentLinesState>();
		private static readonly Subject<ePurgeReagentLinesState> _purgeReagentsStatusCallbackSubject = new Subject<ePurgeReagentLinesState>();
		private static readonly Subject<eFlushFlowCellState> _cleanFluidicsStatusCallbackSubject = new Subject<eFlushFlowCellState>();
		private static readonly Subject<eDecontaminateFlowCellState> _decontaminateStatusCallbackSubject = new Subject<eDecontaminateFlowCellState>();
		
	#endregion

	#region Constructor

		public MaintenanceService(
			ILogger logger,
			DustReferenceModel dustReferenceModel,
			ReagentModel reagentModel)
		{
			_log = logger;
			_dustReferenceModel = dustReferenceModel;
			_reagentModel = reagentModel;

			_reagentModel.CleanFluidicsStateChanged += OnCleanFluidicsStatusCallback;
			_reagentModel.PrimeReagentStateChanged += OnPrimeReagentsStatusCallback;
			_reagentModel.PurgeReagentStateChanged += OnPurgeReagentsStatusCallback;
			_reagentModel.FlowCellDecontaminateStateChanged += OnDecontaminateStatusCallback;
		}

		protected override void DisposeManaged()
		{
			_dustReferenceModel?.Dispose();
			_reagentModel?.Dispose();
			_primeReagentsStatusCallbackSubject?.OnCompleted();
			_purgeReagentsStatusCallbackSubject?.OnCompleted();
			_cleanFluidicsStatusCallbackSubject?.OnCompleted();
			_decontaminateStatusCallbackSubject?.OnCompleted();

			base.DisposeManaged();
		}

		protected override void DisposeUnmanaged()
		{
			base.DisposeUnmanaged();
		}

	#endregion

	#region Clean Fluidics Status subscription

		private readonly object _cleanFluidicsStatusLock = new object();

		public IObservable<eFlushFlowCellState> SubscribeCleanFluidicsStatus()
		{
			return _cleanFluidicsStatusCallbackSubject;
		}

		private void OnCleanFluidicsStatusCallback(object sender, ApiEventArgs<eFlushFlowCellState> args)
		{
			var argMsg = args.Arg1;
			// Keep for debugging:_log.Debug($"Sample Status Callback [SERVICE]::args.Arg1:{argMsg}");
			PublishCleanFluidicsStatus(argMsg);
		}

		private void PublishCleanFluidicsStatus(eFlushFlowCellState status)
		{
			try
			{
				lock (_cleanFluidicsStatusLock)
				{
					_cleanFluidicsStatusCallbackSubject.OnNext(status);
				}
			}
			catch (Exception ex)
			{
				_log.Error(ex, $"Failed to publish CleanFluidics Status callback");

				lock (_cleanFluidicsStatusLock)
				{
					_cleanFluidicsStatusCallbackSubject.OnError(ex);
				}
			}
		}

	#endregion

	#region Prime Reagents Status subscription

		private readonly object _primeReagentsStatusLock = new object();

		public IObservable<ePrimeReagentLinesState> SubscribePrimeReagentsStatus()
		{
			return _primeReagentsStatusCallbackSubject;
		}

		private void OnPrimeReagentsStatusCallback(object sender, ApiEventArgs<ePrimeReagentLinesState> args)
		{
			var argMsg = args.Arg1;
			// Keep for debugging:_log.Debug($"Sample Status Callback [SERVICE]::args.Arg1:{argMsg}");
			PublishPrimeReagentsStatus(argMsg);
		}

		private void PublishPrimeReagentsStatus(ePrimeReagentLinesState status)
		{
			try
			{
				lock (_primeReagentsStatusLock)
				{
					_primeReagentsStatusCallbackSubject.OnNext(status);
				}
			}
			catch (Exception ex)
			{
				_log.Error(ex, $"Failed to publish Prime Reagents Status callback");

				lock (_primeReagentsStatusLock)
				{
					_primeReagentsStatusCallbackSubject.OnError(ex);
				}
			}
		}

	#endregion
	
	#region Purge Reagents Status subscription

		private readonly object _purgeReagentsStatusLock = new object();

		public IObservable<ePurgeReagentLinesState> SubscribePurgeReagentsStatus()
		{
			return _purgeReagentsStatusCallbackSubject;
		}

		private void OnPurgeReagentsStatusCallback(object sender, ApiEventArgs<ePurgeReagentLinesState> args)
		{
			var argMsg = args.Arg1;
			// Keep for debugging:_log.Debug($"Sample Status Callback [SERVICE]::args.Arg1:{argMsg}");
			PublishPurgeReagentsStatus(argMsg);
		}

		private void PublishPurgeReagentsStatus(ePurgeReagentLinesState status)
		{
			try
			{
				lock (_purgeReagentsStatusLock)
				{
					_purgeReagentsStatusCallbackSubject.OnNext(status);
				}
			}
			catch (Exception ex)
			{
				_log.Error(ex, $"Failed to publish Prime Reagents Status callback");

				lock (_purgeReagentsStatusLock)
				{
					_purgeReagentsStatusCallbackSubject.OnError(ex);
				}
			}
		}

	#endregion

	#region Decontaminate Status subscription

		private readonly object _decontaminateStatusLock = new object();

		public IObservable<eDecontaminateFlowCellState> SubscribeDecontaminateStatus()
		{
			return _decontaminateStatusCallbackSubject;
		}

		private void OnDecontaminateStatusCallback(object sender, ApiEventArgs<eDecontaminateFlowCellState> args)
		{
			var argMsg = args.Arg1;
			// Keep for debugging:_log.Debug($"Sample Status Callback [SERVICE]::args.Arg1:{argMsg}");
			PublishDecontaminateStatus(argMsg);
		}

		private void PublishDecontaminateStatus(eDecontaminateFlowCellState status)
		{
			try
			{
				lock (_decontaminateStatusLock)
				{
					_decontaminateStatusCallbackSubject.OnNext(status);
				}
			}
			catch (Exception ex)
			{
				_log.Error(ex, $"Failed to publish Prime Reagents Status callback");

				lock (_decontaminateStatusLock)
				{
					_decontaminateStatusCallbackSubject.OnError(ex);
				}
			}
		}

		#endregion


	#region OPCUA methods called from ScoutOpcUaGrpcService class.

		public bool StartCleanFluidics()
		{
			var result = _reagentModel.StartCleanFluidics();
			return result == HawkeyeError.eSuccess;
		}

		public bool PrimeReagents()
		{
			var result = _reagentModel.StartPrimeReagentLines();
			return result == HawkeyeError.eSuccess;
		}

		public bool CancelPrimeReagents()
		{
			var result = _reagentModel.CancelPrimeReagentLines();
			return result == HawkeyeError.eSuccess;
		}
		
		public bool PurgeReagents()
		{
			var result = _reagentModel.StartPurgeReagentLines();
			return result == HawkeyeError.eSuccess;
		}

		public bool CancelPurgeReagents()
		{
			var result = _reagentModel.CancelPurgeReagentLines();
			return result == HawkeyeError.eSuccess;
		}

		public bool Decontaminate()
		{
			var result = _reagentModel.StartDecontaminateFlowCell();
			return result == HawkeyeError.eSuccess;
		}

		public bool CancelDecontaminate()
		{
			var result = _reagentModel.CancelDecontaminateFlowCell();
			return result == HawkeyeError.eSuccess;
		}
	}

	#endregion
}
