set majorVersion=%1
set minorVersion=%2
set incrementalVersion=%3
set copyrightYear=%4

del /q Directory.Build.props

echo ^<Project^> >> Directory.Build.props
echo 	^<PropertyGroup^> >> Directory.Build.props
echo 		^<Company^>Beckman Coulter Life Sciences^</Company^> >> Directory.Build.props
echo 		^<Copyright^>Copyright (C) %copyrightYear% Beckman Coulter Life Sciences. All rights reserved.^</Copyright^> >> Directory.Build.props
echo 		^<version^>%majorVersion%.%minorVersion%.%incrementalVersion%^</version^> >> Directory.Build.props
echo 		^<assemblyversion^>%majorVersion%.%minorVersion%.%incrementalVersion%^</assemblyversion^> >> Directory.Build.props
echo 	^</PropertyGroup^> >> Directory.Build.props
echo ^</Project^> >> Directory.Build.props


set version_file="ScoutUtilities\UIConfiguration\version.cs"
echo file: %version_file%
del /q %version_file%

echo ^namespace ScoutUtilities.UIConfiguration^ >> %version_file%
echo ^{^ >> %version_file%
echo ^	public partial class UISettings^ >> %version_file%
echo ^	{^ >> %version_file%
echo ^		public static string SoftwareVersion^ >> %version_file%
echo ^		{^ >> %version_file%
echo ^			get { return "%majorVersion%.%minorVersion%.%incrementalVersion%"; }^ >> %version_file%
echo ^		}^ >> %version_file%
echo ^		public static string CopyrightYear^ >> %version_file%
echo ^		{^ >> %version_file%
echo ^			get { return "%copyrightYear%"; }^ >> %version_file%
echo ^		}^ >> %version_file%
echo ^	}^ >> %version_file%
echo ^}^ >> %version_file%
