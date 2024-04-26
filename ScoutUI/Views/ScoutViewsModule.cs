using Ninject.Extensions.Factory;
using Ninject.Modules;
using ScoutUI.Views.Dialogs;

namespace ScoutUI.Views
{
    public class ScoutViewsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IScoutViewFactory>().ToFactory();
            Bind<DialogEventManager>().ToSelf().InTransientScope();
        }
    }
}
