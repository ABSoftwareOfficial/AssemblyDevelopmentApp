using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyDevelopmentApp.Data
{
    public class ProjectFiles
    {
        /// <summary>
        /// All of the Assembly Files that this project will contain.
        /// </summary>
        public List<ProjectFile> AssemblyFiles { get; set; }

        /// <summary>
        /// All of the C Files that this project will contain.
        /// </summary>
        public List<ProjectFile> CFiles { get; set; } = new List<ProjectFile>();

        public ProjectFiles(List<ProjectFile> assemblyFiles, List<ProjectFile> cFiles)
        {
            AssemblyFiles = assemblyFiles;
            CFiles = cFiles;
        }
    }
}