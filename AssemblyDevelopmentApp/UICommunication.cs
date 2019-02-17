using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyDevelopmentApp
{
    /// <summary>
    /// The way the ViewModel can ask the UI to do things (such as open a dialog box)
    /// </summary>
    public abstract class UICommunication
    {
        /// <summary>
        /// Opens an open dialog box, possibly starting at the specified path.
        /// </summary>
        /// <param name="startPath">The path the dialog box should start at.</param>
        /// <returns>The path that the dialog box gave back.</returns>
        public abstract string OpenDialogBoxReturnsPath(string startPath, bool hasStartPath, string fileType, string fileTypeDesc);

        /// <summary>
        /// Opens a save dialog box, possibly starting at the specified path.
        /// </summary>
        /// <param name="startPath">The path the dialog box should start at.</param>
        /// <returns>The path that the dialog box gave back.</returns>
        public abstract string SaveDialogBoxReturnsPath(string startPath, bool hasStartPath, string fileType, string fileTypeDesc);
    }
}
