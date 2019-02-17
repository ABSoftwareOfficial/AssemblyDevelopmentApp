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
    public class ApplicationViewModel : BaseViewModel
    {
        internal string _projectPath;
        private bool _projectIsSuccessfullyLoaded;

        /// <summary>
        /// The current page.
        /// </summary>
        public AppPages CurrentPage { get; set; } = AppPages.Project;

        /// <summary>
        /// The page to move onto after the save changes dialog.
        /// </summary>
        public AppPages MoveOnCurrentPage { get; set; }

        /// <summary>
        /// The current project we're running from.
        /// </summary>
        public ProjectViewModel CurrentProject { get; set; }

        /// <summary>
        /// The path to the current project.
        /// </summary>
        public string ProjectPath
        {
            get => _projectPath;
            set
            {
                _projectPath = value;

                // Attempt to load the new project.
                ProjectIsSuccessfullyLoaded = SaveSystem.LoadProject(value);

                // Whether it failed or not, if it isn't just nothing, it has at least attempted to load the project.
                if (!AttemptedToLoadProject && !string.IsNullOrEmpty(_projectPath))
                    AttemptedToLoadProject = true;
            }
        }

        /// <summary>
        /// Whether the current project path is valid - used after the path that was set in the "ProjectName".
        /// </summary>
        public bool ProjectIsSuccessfullyLoaded
        {
            get => _projectIsSuccessfullyLoaded;
            set
            {
                _projectIsSuccessfullyLoaded = value;

                // Make it so that the "ProjectNotSucessfullyLoaded" is also updated, also, the project if it was loaded, the project has now been loaded, so keep that up-to-date.
                InvokePropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(ProjectNotSucessfullyLoaded)));

                // If the project was not successfuly loaded, go to the project page.
                if (!value)
                    CurrentPage = AppPages.Project;
            }
        }

        /// <summary>
        /// When the project is not loaded.
        /// </summary>
        public bool ProjectNotSucessfullyLoaded
        {
            get => !ProjectIsSuccessfullyLoaded;
        }

        /// <summary>
        /// Whether the project has attempted to load at least once.
        /// </summary>
        public bool AttemptedToLoadProject { get; set; }

        /// <summary>
        /// Whether we can move to a different page.
        /// </summary>
        public bool CanMovePage { get; set; } = true;

        #region Commands

        public ICommand NewProjectCommand { get; set; }
        public ICommand OverwriteNewProjectCommand { get; set; }
        public ICommand BrowseNewProjectCommand { get; set; }
        public ICommand CancelNewProjectCommand { get; set; }

        public ICommand OpenProjectCommand { get; set; }
        public ICommand SaveProjectCommand { get; set; }
        public ICommand SaveProjectAsCommand { get; set; }

        public ICommand OpenProjectPage { get; set; }
        public ICommand OpenFilesPage { get; set; }
        public ICommand OpenExecutePage { get; set; }
        public ICommand OpenABWorldWebsite { get; set; }

        public ICommand DoNotSaveChanges { get; set; }
        public ICommand SaveChanges { get; set; }

        public ICommand ViewScriptingHelp { get; set; }

        #endregion

        public ApplicationViewModel()
        {

            // Create the ProjectPage relay command.
            OpenProjectPage = new RelayCommand(() => ChangePage(AppPages.Project));
            OpenFilesPage = new RelayCommand(() => ChangePage(AppPages.Files));

            // Create the ExecutePage relay command.
            OpenExecutePage = new RelayCommand(() => ChangePage(AppPages.Execute));

            // Create the NewProject relay command.
            NewProjectCommand = new RelayCommand(() =>
            {
                // If the path for the current project is completely valid, ask them what they want to do.
                if (Directory.Exists(Path.GetPathRoot(ProjectPath)) && File.Exists(ProjectPath))
                {
                    CurrentPage = AppPages.Overwrite;
                    CanMovePage = false;
                }

                // Otherwise, if there is no file already specified, there's no point in even looking at that, so go straight to browse.
                else
                {
                    BrowseNewProjectCommand.Execute(null);
                    CurrentPage = AppPages.Project; // Reload the project page
                }

            });

            // Create the BrowseNewProject relay command.
            BrowseNewProjectCommand = new RelayCommand(() =>
            {
                // Browse to a path.
                var path = Backend.UICommunication.SaveDialogBoxReturnsPath(ProjectPath, ProjectIsSuccessfullyLoaded, SaveSystem.ProjectExtension, SaveSystem.ProjectExtensionDescription);

                // Try that path, if it fails, flag it up as attempting to load, if it was valid, we'll make it the ProjectPath.
                if (SaveSystem.NewProject(path))
                    ProjectPath = path;
                else
                    AttemptedToLoadProject = true;

                // Make sure we're back on the "Project" page, and that we can move around again.
                CurrentPage = AppPages.Project;
                CanMovePage = true;
            });

            // Create the OverwriteNewProject relay command.
            OverwriteNewProjectCommand = new RelayCommand(() =>
            {
                // Overwrite the project.
                File.Delete(ProjectPath);
                SaveSystem.NewProject(ProjectPath);

                // Make sure we're back on the "Project" page, and that we can move around again.
                CurrentPage = AppPages.Project;
                CanMovePage = true;

            });

            // Create the relay command to cancel the "New Project" dialog.
            CancelNewProjectCommand = new RelayCommand(() =>
            {
                CurrentPage = AppPages.Project;
                CanMovePage = true;
            });

            // Create the OpenProject relay command.
            OpenProjectCommand = new RelayCommand(() =>
            {
                // Browse to a path.
                var path = Backend.UICommunication.OpenDialogBoxReturnsPath(ProjectPath, ProjectIsSuccessfullyLoaded, SaveSystem.ProjectExtension, SaveSystem.ProjectExtensionDescription);

                // Attempt to load it, if the path is invalid, it did ATTEMPT to load the project.
                if (SaveSystem.LoadProject(path))
                {
                    ProjectPath = path;
                    Backend.UnsavedChangesMade = false;
                }
                else
                    AttemptedToLoadProject = true;

            });

            // Create the SaveProject relay command.
            SaveProjectCommand = new RelayCommand(() =>
            {

                // Attempt to save to the current project path, if we can't, get the user to browse to a folder.
                if (!SaveSystem.SaveProject(CurrentProject.ToData()))
                    SaveProjectCommand.Execute(null);

                // Mark us as now saving the project.
                else
                    Backend.UnsavedChangesMade = false;

            });

            // Create the SaveProjectAs relay command.
            SaveProjectAsCommand = new RelayCommand(() =>
            {
                // Browse to a path.
                var path = Backend.UICommunication.SaveDialogBoxReturnsPath(ProjectPath, ProjectIsSuccessfullyLoaded, SaveSystem.ProjectExtension, SaveSystem.ProjectExtensionDescription);

                // Check if the path is valid if it is, create a file and save.
                if (!string.IsNullOrEmpty(path) || Directory.Exists(Path.GetDirectoryName(path)))
                {
                    File.WriteAllText(path, "");

                    // Save the project and make it the new path.
                    SaveSystem.SaveProjectToPath(CurrentProject.ToData(), path);
                    ProjectPath = path;
                }

                // If it wasn't valid, we did attempt to load it at least.
                else
                    AttemptedToLoadProject = true;
            });

            // The code to execute when not saving.
            DoNotSaveChanges = new RelayCommand(() =>
            {
                // Move onto the next page, and mark us as no longer having not saved changes.
                CurrentPage = MoveOnCurrentPage;
                Backend.UnsavedChangesMade = false;

                // Make it so that we can move page again.
                CanMovePage = true;

            });

            // The code to execute when saving.
            SaveChanges = new RelayCommand(() =>
            {
                // Move onto the next page, and mark us as no longer having not saved changes.
                CurrentPage = MoveOnCurrentPage;
                Backend.UnsavedChangesMade = false;

                // Save the project now.
                SaveSystem.SaveProject(CurrentProject.ToData());

                // Make it so that we can move page again.
                CanMovePage = true;
            });

            // Create the code to open the ABWorld website.
            OpenABWorldWebsite = new RelayCommand(() => Process.Start("http://abworld.ml"));

            ViewScriptingHelp = new RelayCommand(() => CurrentPage = AppPages.ScriptingHelp);
        }

        public void ChangePage(AppPages page)
        {
            // If we can't move page, don't go anywhere
            if (!CanMovePage)
                return;

            // Check if we've made unsaved changes to the current ViewModel - if so, we'll go to the "Save" page.
            if (Backend.UnsavedChangesMade)
            {
                MoveOnCurrentPage = page;
                CurrentPage = AppPages.Save;

                // We can also no longer move page.
                CanMovePage = false;
            }

            // Finally, if we have saved all changes, change the page.
            else CurrentPage = page;
        }
    }
}
