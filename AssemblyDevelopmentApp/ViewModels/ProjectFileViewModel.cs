using AssemblyDevelopmentApp.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AssemblyDevelopmentApp.ViewModels
{
    public class ProjectFileViewModel : BaseViewModel
    {
        public ProjectFilesViewModel Parent;

        string _inputFilePath;
        string _outputFilePath;

        /// <summary>
        /// The title for this file.
        /// </summary>
        public string FileTitle { get; set; }

        /// <summary>
        /// Whether this represents an assembly file or not.
        /// </summary>
        public bool IsAssemblyFile { get; set; }

        /// <summary>
        /// The input path to this file.
        /// </summary>
        public string InputFilePath
        {
            get => _inputFilePath;
            set
            {
                _inputFilePath = value;

                // Check if the path is valid, whether it is or not will set the "ValidInputFilePath".
                ValidInputFilePath = File.Exists(InputFilePath);
            }
        }

        /// <summary>
        /// The output path to this file.
        /// </summary>
        public string OutputFilePath
        {
            get => _outputFilePath;
            set
            {
                _outputFilePath = value;

                // Decide whether the path is valid for saving or not.
                try
                {
                    if (string.IsNullOrEmpty(_outputFilePath) || !Directory.Exists(Path.GetDirectoryName(_outputFilePath)))
                        ValidOutputFilePath = false;
                    else
                        ValidOutputFilePath = true;
                }

                // If it failed to even check if it was valid, that means it can't be used at all.
                catch { ValidOutputFilePath = false; }
            }
        }

        /// <summary>
        /// Whether the input file path is valid.
        /// </summary>
        public bool ValidInputFilePath { get; set; }

        /// <summary>
        /// Whether the output file path is valid.
        /// </summary>
        public bool ValidOutputFilePath { get; set; }

        /// <summary>
        /// Allows the user to browse for the input path.
        /// </summary>
        public ICommand BrowseInputPath { get; set; }

        /// <summary>
        /// Allows the user to browse for the output path.
        /// </summary>
        public ICommand BrowseOutputPath { get; set; }

        /// <summary>
        /// Allows the user to open up the input path.
        /// </summary>
        public ICommand OpenInputPath { get; set; }

        /// <summary>
        /// Allows the user to browse for the output path.
        /// </summary>
        public ICommand OpenOutputPath { get; set; }

        /// <summary>
        /// Deletes this item.
        /// </summary>
        public ICommand DeleteItem { get; set; }

        public ProjectFileViewModel(ProjectFilesViewModel parent)
        {
            InitalizeViewModel();
            Parent = parent;
        }

        public ProjectFileViewModel(string fileTitle, string inputFilePath, string outputFilePath, ProjectFilesViewModel parent, bool isAssemblyFile)
        {
            FileTitle = fileTitle;
            InputFilePath = inputFilePath;
            OutputFilePath = outputFilePath;
            IsAssemblyFile = isAssemblyFile;
            Parent = parent;

            // Initialize all the basic ViewModel functionality
            InitalizeViewModel();
        }

        public void InitalizeViewModel()
        {
            // Configure the browse input path code.
            BrowseInputPath = new RelayCommand(() =>
            {
                // Get the user to browse to a location.
                var path = Backend.UICommunication.OpenDialogBoxReturnsPath("", false, IsAssemblyFile ? "*.asm" : "*.c", IsAssemblyFile ? "Assembly File" : "C File");

                // If the path was valid, make that the path.
                if (File.Exists(path))
                    InputFilePath = path;

            });

            // Configure the browse output path code.
            BrowseOutputPath = new RelayCommand(() =>
            {
                // Get the user to browse to a location.
                var path = Backend.UICommunication.SaveDialogBoxReturnsPath("", false, "*.o", "Output File");

                // Check if the path could be valid
                try
                {
                    if (string.IsNullOrEmpty(path) || Directory.Exists(Path.GetDirectoryName(path)))
                        OutputFilePath = path;
                }
                catch { }

            });

            // Configure the go to input code.
            OpenInputPath = new RelayCommand(() =>
            {
                // Open up that path via explorer - if it's valid.
                if (ValidInputFilePath)
                    Process.Start("explorer", Path.GetDirectoryName(InputFilePath));
            });

            // Configure the go to output code.
            OpenOutputPath = new RelayCommand(() =>
            {
                // Open up that path via explorer - if it's valid.
                if (ValidOutputFilePath)
                    Process.Start("explorer", Path.GetDirectoryName(OutputFilePath));

            });

            // Configure the code to delete this item.
            DeleteItem = new RelayCommand(() =>
            {
                if (IsAssemblyFile)
                    Parent.AssemblyFiles.Remove(this);
                else
                    Parent.CFiles.Remove(this);
            });

            // Implement a way of keeping track of whether we've made changes to the assembly file or not.
            PropertyChanged += (s, e) =>
                Backend.UnsavedChangesMade = true;
        }

        public ProjectFile ToData()
        {
            return new ProjectFile(FileTitle, InputFilePath, OutputFilePath);
        }

        public static ProjectFileViewModel FromData(ProjectFile file, ProjectFilesViewModel project, bool isAssemblyFile)
        {
            return new ProjectFileViewModel(file.FileTitle, file.InputFilePath, file.OutputFilePath, project, isAssemblyFile);
        }
    }
}
