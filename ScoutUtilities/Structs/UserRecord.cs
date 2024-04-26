using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScoutUtilities.Enums;
using System.Runtime.InteropServices;

namespace ScoutUtilities.Structs
{
	[StructLayout(LayoutKind.Sequential)]
	public struct UserRecord
	{
		public string UserName;
		public string DisplayName;
		public string Comments;
		public string Email;
		public string SampleExportFolder;
        public string CsvExportFolder;
        public string LangCode;
        public string DefaultResultFileName;
        public string DefaultSampleName;

        public Int16 DefaultImageSaveN;
        public Int16 DefaultWashType;
        public Int16 DefaultDilution;
        public UInt32 DefaultCellTypeIndex;
        public bool ExportPdfEnabled;

        public UInt32 DisplayDigits;
	    public bool AllowFastMode;

		public UserPermissionLevel PermissionLevel;

	};
}
