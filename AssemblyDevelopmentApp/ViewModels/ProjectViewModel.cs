using AssemblyDevelopmentApp.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AssemblyDevelopmentApp.ViewModels
{
    public class ProjectViewModel : BaseViewModel
    {
        private string _finalOutput;
        private string _linkFile;
        /// <summary>
        /// The name for the project.
        /// </summary>
        public string ProjectName { get; set; }

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
        /// The bash to execute for each assembly file.
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
        public ProjectFilesViewModel Files { get; set; }

        /// <summary>
        /// The output from cygwin.
        /// </summary>
        public string CygwinOutput { get; set; }

        /// <summary>
        /// The link file for the project.
        /// </summary>
        public string LinkFile
        {
            get => _linkFile;
            set
            {
                _linkFile = value;

                // If the path was valid, mark it as valid, otherwise, it isn't valid.
                if (File.Exists(value))
                    LinkFileIsValid = true;
                else
                    LinkFileIsValid = false;
            }
        }

        /// <summary>
        /// Whether the link file is valid.
        /// </summary>
        public bool LinkFileIsValid { get; set; }

        /// <summary>
        /// The final output for the project.
        /// </summary>
        public string FinalOutput
        {
            get => _finalOutput;
            set
            {
                _finalOutput = value;

                // If the path was valid, mark it as valid and make that the path.
                try
                {
                    if (!string.IsNullOrEmpty(value) && Directory.Exists(Path.GetDirectoryName(value)))
                    {
                        FinalOutputIsValid = true;
                        return;
                    }
                }
                catch { }

                // Otherwise, if we got here, the final output wasn't valid.
                FinalOutputIsValid = false;
            }
        }

        /// <summary>
        /// Whether the final output for the project is valid.
        /// </summary>
        public bool FinalOutputIsValid { get; set; }

        public ICommand BuildProject { get; set; }
        public ICommand TestProject { get; set; }
        public ICommand BothProject { get; set; }

        public ICommand BrowseLinkFile { get; set; }
        public ICommand BrowseFinalOutput { get; set; }

        /// <summary>
        /// Whether all of the files in the project are valid.
        /// </summary>
        public bool HasInvalidFiles
        {
            get
            {
                // Go through each file and look for one with either a failed input or output, if we don't find one, then it doesn't have invalid files.
                for (int i = 0; i < Files.AssemblyFiles.Count; i++)
                    if (!Files.AssemblyFiles[i].ValidInputFilePath || !Files.AssemblyFiles[i].ValidOutputFilePath)
                        return true;

                return false;
            }
        }

        public bool HasInvalidFinalOutputOrLink
        {
            get
            {
                if (!FinalOutputIsValid || !LinkFileIsValid)
                    return true;

                else return false;
            }
        }

        public ProjectViewModel(string projectName, ProjectFilesViewModel files, string finalOutput, string linkFile, string cygwinStartUp, string cygwinStop, string buildStart, string buildEachAsssembly, string buildEachC, string buildFinish, string testCode)
        {
            ProjectName = projectName;
            Files = files;
            FinalOutput = finalOutput;
            LinkFile = linkFile;
            CygwinStartUp = cygwinStartUp;
            CygwinStop = cygwinStop;
            BuildStart = buildStart;
            BuildEachAssembly = buildEachAsssembly;
            BuildEachC = buildEachC;
            BuildFinish = buildFinish;
            TestCode = testCode;

            BuildProject = new RelayCommand(async () => CygwinOutput = await ProjectExecution.BuildProject(ToData()));
            TestProject = new RelayCommand(async () => CygwinOutput = await ProjectExecution.TestProject(ToData()));
            BothProject = new RelayCommand(async () => CygwinOutput = await ProjectExecution.BuildAndTestProject(ToData()));

            // Implement the methods for browsing to the LinkFile and FinalOutput
            BrowseFinalOutput = new RelayCommand(() =>
            {
                var path = Backend.UICommunication.SaveDialogBoxReturnsPath("", false, "*.bin", "Binary File");

                try
                {
                    if (!string.IsNullOrEmpty(path) && Directory.Exists(Path.GetDirectoryName(path)))
                        FinalOutput = path;
                }
                catch { }

            });

            BrowseLinkFile = new RelayCommand(() =>
            {
                var path = Backend.UICommunication.OpenDialogBoxReturnsPath("", false, "*.ld", "Link File");

                try
                {
                    if (!string.IsNullOrEmpty(path) && Directory.Exists(Path.GetDirectoryName(path)))
                        LinkFile = path;
                }
                catch { }

            });

            // Implement a way of keeping track of whether we've made changes to the project or not.
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != nameof(CygwinOutput))
                    Backend.UnsavedChangesMade = true;
            };
        }

        /// <summary>
        /// Converts this ViewModel into a piece of data.
        /// </summary>
        public Project ToData()
        {
            return new Project(ProjectName, Files.ToData(), LinkFile, FinalOutput, CygwinStartUp, CygwinStop, BuildStart, BuildEachAssembly, BuildEachC, BuildFinish, TestCode);
        }

        public static ProjectViewModel FromData(Project proj)
        {
            return new ProjectViewModel(proj.ProjectName, ProjectFilesViewModel.FromData(proj.Files), proj.FinalOutput, proj.LinkFile, proj.CygwinStartUp, proj.CygwinStop, proj.BuildStart, proj.BuildEachAssembly, proj.BuildEachC, proj.BuildFinish, proj.TestCode);
        }
    }
}
