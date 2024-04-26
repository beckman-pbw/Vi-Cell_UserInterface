using System;
using System.Reactive.Subjects;
using Ninject.Extensions.Logging;
using ScoutLanguageResources;
using ScoutModels.Common;
using ScoutModels.Interfaces;

namespace ScoutModels.Service
{
    public class ApplicationStateService : IApplicationStateService
    {
        private readonly Subject<ApplicationStateChange> _applicationStateChangeSubject;
        private readonly ILogger _logger;

        public ApplicationStateService(ILogger logger)
        {
            _logger = logger;
            _applicationStateChangeSubject = new Subject<ApplicationStateChange>();
        }

        public IDisposable SubscribeStateChanges(Action<ApplicationStateChange> onNext)
        {
            return _applicationStateChangeSubject.Subscribe(onNext);
        }

        public void PublishStateChange(ApplicationStateEnum newState, string reason, bool restart)
        {
            if (null == reason)
            {
                reason = LanguageResourceHelper.Get("LID_MSGBOX_ExitProgressMsg");
            }

            var change = new ApplicationStateChange(newState, reason, restart);
            _applicationStateChangeSubject.OnNext(change);
        }
    }
}