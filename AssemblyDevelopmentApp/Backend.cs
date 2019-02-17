using AssemblyDevelopmentApp.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyDevelopmentApp
{
    /// <summary>
    /// Handles the main ViewModel and selects the correct ViewModel (for the DataContext) to use for the current page.
    /// </summary>
    public static class Backend
    {
        /// <summary>
        /// The way to communicate with the UI.
        /// </summary>
        public static UICommunication UICommunication { get; set; }

        /// <summary>
        /// The main instance of "ApplicationViewModel" for the application.
        /// </summary>
        public static ApplicationViewModel MainAppViewModel { get; set; } = new ApplicationViewModel();

        /// <summary>
        /// Whether the current page has had unsaved changes.
        /// </summary>
        public static bool UnsavedChangesMade { get; set; }

        /// <summary>
        /// The view model for the current page.
        /// </summary>
        public static BaseViewModel CurrentPageViewModel
        {
            get
            {
                switch (MainAppViewModel.CurrentPage)
                {
                    case AppPages.Project: // Project
                        return MainAppViewModel;
                    case AppPages.Files: // Files
                        return MainAppViewModel.CurrentProject.Files;
                    case AppPages.Execute: // Execute
                        return MainAppViewModel.CurrentProject;
                    case AppPages.Save: // Save
                        return MainAppViewModel;
                    case AppPages.Overwrite: // Overwrite
                        return MainAppViewModel;
                    case AppPages.ScriptingHelp:
                        return MainAppViewModel;
                    default:
                        throw new Exception("Invalid Page.");
                }
            }
        }
    }
}
