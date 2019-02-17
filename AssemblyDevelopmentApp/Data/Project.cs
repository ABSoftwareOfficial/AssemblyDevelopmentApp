using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyDevelopmentApp.Data
{
    public class Project
    {
        /// <summary>
        /// The name for the project.
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// The final output for the project.
        /// </summary>
        public string FinalOutput { get; set; }

        /// <summary>
        /// The link file for the project.
        /// </summary>
        public string LinkFile { get; set; }

        /// <summary>
        /// The bash to execute when starting up cygwin in the background.
        /// </summary>
        public string CygwinStartUp { get; set; }

        /// <summary>
        /// The bash to execute when stopping cygwin in the background.
        /// </summary>
        public string CygwinStop { get; set; }

        /// <summary>
        /// The bash to execute when starting to build.
        /// </summary>
        public string BuildStart { get; set; }

        /// <summary>
        /// The bash to execute when finishing building.
        /// </summary>
        public string BuildEachAssembly { get; set; }

        /// <summary>
        /// The bash to execute for each C file.
        /// </summary>
        public string BuildEachC { get; set; }

        /// <summary>
        /// The bash to execute when building each assembly file.
        /// </summary>
        public string BuildFinish { get; set; }

        /// <summary>
        /// The bash to execute when testing the assembly.
        /// </summary>
        public string TestCode { get; set; }

        /// <summary>
        /// Information about the files this project contains.
        /// </summary>
        public ProjectFiles Files { get; set; }

        public Project(string projectName, ProjectFiles files, string linkFile, string finalOutput, string cygwinStartUp, string cygwinStop, string buildStart, string buildEachAsssembly, string buildEachC, string buildFinish, string testCode)
        {
            ProjectName = projectName;
            Files = files;
            CygwinStartUp = cygwinStartUp;
            CygwinStop = cygwinStop;
            BuildStart = buildStart;
            BuildEachAssembly = buildEachAsssembly;
            BuildEachC = buildEachC;
            BuildFinish = buildFinish;
            TestCode = testCode;
            LinkFile = linkFile;
            FinalOutput = finalOutput;
        }
    }
}
