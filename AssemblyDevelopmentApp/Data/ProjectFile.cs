using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyDevelopmentApp.Data
{
    /// <summary>
    /// Represents a file found in a project.
    /// </summary>
    public class ProjectFile
    {
        /// <summary>
        /// The title for this file.
        /// </summary>
        public string FileTitle { get; set; }

        /// <summary>
        /// The input path to this file.
        /// </summary>
        public string InputFilePath { get; set; }

        /// <summary>
        /// The output path to this file.
        /// </summary>
        public string OutputFilePath { get; set; }

        public ProjectFile(string fileTitle, string inputFilePath, string outputFilePath)
        {
            FileTitle = fileTitle;
            InputFilePath = inputFilePath;
            OutputFilePath = outputFilePath;
        }
    }
}
