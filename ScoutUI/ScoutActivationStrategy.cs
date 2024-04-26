using System.Diagnostics;
using Ninject.Activation.Strategies;
using Ninject.Extensions.Logging;

namespace ScoutUI
{
    public class ScoutActivationStrategy : ActivationStrategy
    {
        private ILogger _logger;

        public override void Activate(Ninject.Activation.IContext context, Ninject.Activation.InstanceReference reference)
        {
            if (reference.Instance is ILogger)
            {
                _logger = (ILogger)reference.Instance;
            }
            _logger?.Debug("Ninject Activate: " + reference?.Instance?.GetType());
            Debug.WriteLine("Ninject Activate: " + reference?.Instance?.GetType());
            base.Activate(context, reference);
        }

        public override void Deactivate(Ninject.Activation.IContext context, Ninject.Activation.InstanceReference reference)
        {
            _logger?.Debug("Ninject DeActivate: " + reference?.Instance?.GetType());
            Debug.WriteLine("Ninject DeActivate: " + reference?.Instance?.GetType());
            base.Deactivate(context, reference);
        }
    }
}