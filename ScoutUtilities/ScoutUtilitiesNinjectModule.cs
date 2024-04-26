using Ninject.Modules;
using ScoutUtilities.Events;
using ScoutUtilities.Services;

namespace ScoutUtilities
{
    public class ScoutUtilitiesNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IFileSystemService>().To<FileSystemService>().InTransientScope();
            Bind<IDialogCaller>().To<DialogCaller>().InTransientScope();
        }
    }
}