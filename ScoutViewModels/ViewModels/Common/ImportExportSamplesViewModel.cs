using ScoutModels;
using ScoutUtilities.Common;
using System;
using System.IO;
using System.Windows.Forms;

namespace ScoutViewModels.ViewModels.Common
{
    public class ImportExportSamplesViewModel : BaseViewModel
    {
	    public static string ExportAdvanceSetting(string path)
        {
            var saveFileDialog = new FolderBrowserDialog
            {
                SelectedPath = path
            };
            return saveFileDialog.ShowDialog() == DialogResult.OK ? saveFileDialog.SelectedPath : path;
        }
    }
}