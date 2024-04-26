// ***********************************************************************
// <copyright file="InstanceCreator.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Reflection;

namespace ScoutUtilities
{
   
    public class InstanceCreator
    {
        public static T CreateInstance<T>(string assemblyName, string className) where T : class
        {
            var assemblyPath = Environment.CurrentDirectory + "\\" + assemblyName;
            var assembly = Assembly.LoadFrom(assemblyPath);
            var type = assembly.GetType(className);
            return Activator.CreateInstance(type) as T;
        }
    }
}