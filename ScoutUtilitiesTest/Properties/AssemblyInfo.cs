using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Vi-CELL BLU")]
[assembly: AssemblyDescription("Cell viability analyzer")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Beckman Coulter Life Sciences")]
[assembly: AssemblyProduct("Vi-CELL BLU")]
[assembly: AssemblyCopyright("Copyright (C) ${current.year} Beckman Coulter Life Sciences. All rights reserved.")]
[assembly: AssemblyTrademark("Beckman Coulter, the stylized logo, and the Beckman Coulter product and service marks mentioned herein are trademarks or registered trademarks of Beckman Coulter, Inc. in the United States and other countries.")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("3a303327-070e-4d66-99bf-347fa88053b7")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("${parsedVersion.majorVersion}.${parsedVersion.minorVersion}.${parsedVersion.incrementalVersion}.${timestamp}")]
[assembly: AssemblyFileVersion("${parsedVersion.majorVersion}.${parsedVersion.minorVersion}.${parsedVersion.incrementalVersion}.${timestamp}")]
