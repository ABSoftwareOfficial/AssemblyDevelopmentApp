using AssemblyDevelopmentApp.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AssemblyDevelopmentApp.ViewModels
{
    public class ProjectFilesViewModel : BaseViewModel
    {
        /// <summary>
        /// All of the Assembly Files that this project will contain.
        /// </summary>
        public ObservableCollection<ProjectFileViewModel> AssemblyFiles { get; set; }

        /// <summary>
        /// All of the C Files that this project will contain.
        /// </summary>
        public ObservableCollection<ProjectFileViewModel> CFiles { get; set; }

        /// <summary>
        /// The file that we're currently adding.
        /// </summary>
        public ProjectFileViewModel CurrentAddFile { get; set; }

        /// <summary>
        /// Whether we're adding a file or not.
        /// </summary>
        public bool IsAddingFile { get; set; }

        public ICommand ShowAddMenu { get; set; }
        public ICommand ApplyAddMenuToAssembly { get; set; }
        public ICommand ApplyAddMenuToC { get; set; }

        public ProjectFilesViewModel(ObservableCollection<ProjectFileViewModel> assemblyFiles, ObservableCollection<ProjectFileViewModel> cFiles)
        {
            AssemblyFiles = assemblyFiles;
            CFiles = cFiles;

            ShowAddMenu = new RelayCommand(() =>
            {
                CurrentAddFile = new ProjectFileViewModel(this);
                IsAddingFile = true;
            });

            ApplyAddMenuToAssembly = new RelayCommand(() =>
            {
                CurrentAddFile.IsAssemblyFile = true;
                AssemblyFiles.Add(CurrentAddFile);
                IsAddingFile = false;
            });

            ApplyAddMenuToC = new RelayCommand(() =>
            {
                CFiles.Add(CurrentAddFile);
                IsAddingFile = false;
            });

            // Implement a way of keeping track of whether we've made changes to the project or not.
            PropertyChanged += (s, e) =>
                Backend.UnsavedChangesMade = true;
        }

        /// <summary>
        /// Converts this ViewModel into a piece of data.
        /// </summary>
        public ProjectFiles ToData()
        {
            var newAssembly = new List<ProjectFile>();
            var newC = new List<ProjectFile>();

            // Convert each item to the correct type - for both assembly and C files.
            for (int i = 0; i < AssemblyFiles.Count; i++)
                newAssembly.Add(AssemblyFiles[i].ToData());
            for (int i = 0; i < CFiles.Count; i++)
                newC.Add(CFiles[i].ToData());

            return new ProjectFiles(newAssembly, newC);
        }

        public static ProjectFilesViewModel FromData(ProjectFiles files)
        {
            var newAssembly = new ObservableCollection<ProjectFileViewModel>();
            var newC = new ObservableCollection<ProjectFileViewModel>();

            // Create a new view model to give back.
            var newProjFiles = new ProjectFilesViewModel(newAssembly, newC);

            // Convert each item to the correct type - for both assembly and C files.
            for (int i = 0; i < files.AssemblyFiles.Count; i++)
                newAssembly.Add(ProjectFileViewModel.FromData(files.AssemblyFiles[i], newProjFiles, true));
            for (int i = 0; i < files.CFiles.Count; i++)
                newC.Add(ProjectFileViewModel.FromData(files.CFiles[i], newProjFiles, false));

            // Update the project files and return it.
            return newProjFiles;
        }
    }
}
