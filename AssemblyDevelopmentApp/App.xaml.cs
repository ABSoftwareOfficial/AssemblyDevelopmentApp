using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AssemblyDevelopmentApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configure the UICommunication.
            Backend.UICommunication = new VMCommunication();
        }
    }

    public class VMCommunication : UICommunication
    {
        public override string OpenDialogBoxReturnsPath(string startPath, bool hasStartPath, string fileType, string fileTypeDesc)
        {
            return OpenDialogAndReturnPath(startPath, new OpenFileDialog(), hasStartPath, fileType, fileTypeDesc);
        }

        public override string SaveDialogBoxReturnsPath(string startPath, bool hasStartPath, string fileType, string fileTypeDesc)
        {
            return OpenDialogAndReturnPath(startPath, new SaveFileDialog(), hasStartPath, fileType, fileTypeDesc);
        }

        private string OpenDialogAndReturnPath(string startPath, FileDialog dialog, bool hasStartPath, string fileType, string fileTypeDesc)
        {
            // Configure the file type.
            dialog.Filter = fileTypeDesc + "|" + fileType + "|All Files|*.*";

            // Make the dialog box start at the "startPath", if there is one.
            if (!hasStartPath)
                dialog.InitialDirectory = startPath;

            // Show the dialog box.
            var result = dialog.ShowDialog();

            // If it succeeded, return the path, otherwise, return nothing.
            if (result == true)
                return dialog.FileName;
            else
                return "";
        }
    }
}
