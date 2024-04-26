using ApiProxies.Generic;
using HawkeyeCoreAPI;
using log4net;
using ScoutDomains;
using ScoutDomains.Common;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Events;
using System;
using System.Collections.Generic;
using System.IO;
using HawkeyeCoreAPI.Facade;
using ScoutUtilities.Enums;

namespace ScoutModels.Common
{
    public class ImportExportModel
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string GetFolderPathForImportExport()
        {
            var filePath = Path.Combine(ScoutUtilities.UIConfiguration.UISettings.ExportPath, LoggedInUser.CurrentUserId);
            if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
            return filePath;
        }

        public static void GetFilePathForExport(out string fileName, out string filePath, SubstrateType substrateType)
        {
            filePath = GetFolderPathForImportExport();
            var str = ExportModel.SubstrateAsString(substrateType);
            fileName = str + ScoutUtilities.Misc.ConvertToFileNameFormat(DateTime.Now) + ".dat";
        }

        public static bool PromptForExportLocation(out string fileName, out string filePath, SubstrateType substrateType)
        {
            GetFilePathForExport(out string initialFileName, out string initialFilePath, substrateType);
            var removableDrivePath = RemovableDeviceHelper.GetRemovableDrive();
            var initialDir = !string.IsNullOrEmpty(removableDrivePath) ? removableDrivePath : initialFilePath;
            var args = new SaveFileDialogEventArgs(initialFileName, initialDir,
                "Data Files (*.dat)|*.dat", "dat");
            
            if (DialogEventBus.OpenSaveFileDialog(null, args) == true)
            {
                filePath = args.FullFilePathSelected;
                fileName = Path.GetFileName(args.FullFilePathSelected);
                return true;
            }

            fileName = string.Empty;
            filePath = string.Empty;
            return false;
        }

        public static bool ExportSampleSet(List<SampleDomain> sampleDomains, SubstrateType substrateType, 
            bool usingRowWiseSort, string fileName, string filePath, string encryptionKey)
        {
            var selectedSampleList = ExportModel.GetFrom(sampleDomains, substrateType, usingRowWiseSort);
            ExportLibrary.Export(selectedSampleList, 1, fileName, filePath, encryptionKey, ExceptionHelper.HandleExceptions);
            return true;
        }

        public static bool PromptForImportLocation(out string filePath)
        {
            var initialDir = GetFolderPathForImportExport();
            var removableDrivePath = RemovableDeviceHelper.GetRemovableDrive();
            initialDir = !string.IsNullOrEmpty(removableDrivePath) ? removableDrivePath : initialDir;
            var args = new SelectFileDialogEventArgs(initialDir, "Data Files (*.dat)|*.dat", "dat");

            if (DialogEventBus.OpenSelectFileDialog(null, args) == true)
            {
                filePath = args.FullFilePathSelected;
                return true;
            }

            filePath = string.Empty;
            return false;
        }

        public static List<SampleDomain> ImportSampleSet(string username, string password, string fullFilePath)
        {
            var outputPath = string.Empty;
            try
            {
                var filePath = Path.GetDirectoryName(fullFilePath);
                var fileName = Path.GetFileNameWithoutExtension(fullFilePath);
                if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(fileName))
                    return null;

                var inputFile = Path.Combine(filePath, fileName + ".dat");
                outputPath = Path.Combine(filePath, fileName + ".xml");
                FileSystem.DecryptFile(inputFile, outputPath, ApplicationConstants.ExportEncryptKey);
                var collection = XmlSerializeUtil.DeSerializeFromXml<ExportQueueCreationDomain>(outputPath);

                var allCellTypes = CellTypeFacade.Instance.GetAllCellTypes_BECall();

                if (!ExportDomainHelper.ValidateCellTypeAndQualityControlOnImport(collection, allCellTypes))
                {
                    return null;
                }

                var sampleList = ExportDomainHelper.ConvertExportDomainToSampleList(collection);
                return sampleList;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to import sample set from file: {fullFilePath}", e);
                return null;
            }
            finally
            {
                FileSystem.DeleteFileSafe(outputPath);
            }
        }
    }
}