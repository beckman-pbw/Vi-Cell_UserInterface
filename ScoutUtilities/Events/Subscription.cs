using System;
using System.Reflection;

namespace ScoutUtilities.Events
{
    public class Subscription<Tmessage>
    {
        private readonly MessageBus _messageBus;
        public readonly WeakReference TargetObject;
        public readonly bool IsStatic;
        public readonly MethodInfo MethodInfo;

        public Subscription(Action<Tmessage> action, MessageBus messageBus)
        {
            MethodInfo = action.Method;
            if (action.Target == null) IsStatic = true;
            TargetObject = new WeakReference(action.Target);
            _messageBus = messageBus;
        }

        public Action<Tmessage> CreateAction()
        {
            if (TargetObject.Target != null && TargetObject.IsAlive)
                return (Action<Tmessage>) System.Delegate.CreateDelegate(typeof(Action<Tmessage>), TargetObject.Target, MethodInfo);
            
            if (IsStatic)
                return (Action<Tmessage>) System.Delegate.CreateDelegate(typeof(Action<Tmessage>), MethodInfo);
            
            return null;
        }
    }
}