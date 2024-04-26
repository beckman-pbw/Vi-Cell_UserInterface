using System;
using System.Reflection;

namespace ScoutUtilities.Helper
{
    public class ReflectionHelpers
    {
        /// <summary>
        /// Searches the given MethodInfo to see if it has a CustomAttribute of
        /// type 'T' and returns it (if found). 
        /// </summary>
        /// <typeparam name="T">Must be an Attribute</typeparam>
        /// <param name="method">The method to search for the attribute</param>
        /// <returns>The CustomAttribute of type 'T' for method, if found</returns>
        public static T GetCustomAttribute<T>(MethodInfo method) where T : Attribute
        {
            try
            {
                T foundAttribute = null;
                foreach (var a in method.GetCustomAttributes())
                {
                    if (a is T attribute)
                    {
                        foundAttribute = attribute;
                        break;
                    }
                }

                return foundAttribute;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}