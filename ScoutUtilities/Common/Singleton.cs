using System;
using System.Reflection;

namespace ScoutUtilities.Common
{
    public abstract class Singleton<TSingleton> where TSingleton : class
    {
        private static TSingleton _instance;
        private static readonly object _dblChkLock = new object();
        private static Type[] _ctorParameterTypes = Type.EmptyTypes;
        private static object[] _constructorArguments = new object[0];

        public static object[] ConstructorArguments
        {
            get { return _constructorArguments; }
            set
            {
                _constructorArguments = value;
                if (_constructorArguments != null && _constructorArguments.Length > 0)
                {
                    _ctorParameterTypes = new Type[_constructorArguments.Length];
                    for (int i = 0; i < _constructorArguments.Length; i++)
                    {
                        _ctorParameterTypes[i] = _constructorArguments.GetType();
                    }
                }
            }
        }

        public static TSingleton Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_dblChkLock)
                    {
                        if (_instance == null)
                        {
                            var ctor = GetConstructor();
                            _instance = (TSingleton) ctor.Invoke(_constructorArguments);
                        }
                    }
                }

                return _instance;
            }
        }

        private static ConstructorInfo GetConstructor()
        {
            var ctor = typeof(TSingleton).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, _ctorParameterTypes, null);
            if (ctor == null)
            {
                var msg =
                    $"Constructor with {_ctorParameterTypes.Length} parameters for {typeof(TSingleton).Name} not found.";
                throw new MissingMethodException(msg);
            }

            return ctor;
        }
    }
}