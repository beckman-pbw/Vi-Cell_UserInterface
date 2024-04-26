using Ninject.Extensions.Logging;
using ScoutDomains;
using ScoutModels.Interfaces;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;

namespace ScoutModels.Service
{
    public class DisplayService : IDisplayService
    {
        private ILogger _logger;

        public DisplayService(ILogger logger)
        {
            _logger = logger;
        }

        private void PublishHubMessage(string msg)
        {
            _logger.Warn(msg);
            MessageBus.Default.Publish(new SystemMessageDomain
            {
                IsMessageActive = true,
                Message = msg,
                MessageType = MessageType.Warning
            });
        }

        public void DisplayMessage(string message, bool showPrompt = true)
        {
            if (showPrompt)
                DialogEventBus.DialogBoxOk(null, message);
            PublishHubMessage(message);
        }
    }
}