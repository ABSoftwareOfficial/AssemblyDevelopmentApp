using AssemblyDevelopmentApp.Data;
using AssemblyDevelopmentApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyDevelopmentApp
{
    /// <summary>
    /// A system for handling loading/saving projects.
    /// </summary>
    public static class SaveSystem
    {
        public const string ProjectExtension = "*.asmproj";
        public const string ProjectExtensionDescription = "ABWorld/ABMedia Assembly 'Project'";

        public const string DefaultProjectName = "New Project";
        public const string DefaultCygwinStartUp = @"";

        public const string DefaultCygwinStop = @"";

        public const string DefaultBuildStart = @"rm %FINALOUT% /q";

        public const string DefaultBuildEachAssembly = @"rm %FILEOUT%
nasm -f elf -o %FILEOUT% %FILEIN%";

        public const string DefaultBuildEachC = @"rm %FILEOUT%
i586-elf-gcc -m32 -o %FILEOUT% -c %FILEIN%";

        public const string DefaultBuildFinish = @"i586-elf-ld -m elf_i386 -T %LINKFILE% -o %FINALOUT% %ASMFILESOUT% %CFILESOUT%";

        public const string DefaultTestCode = @"qemu-system-i386 -kernel %FINALOUT%";

        /// <summary>
        /// (Attempts to) create a new project at the given location.
        /// </summary>
        /// <returns>Whether the path was valid.</returns>
        public static bool NewProject(string path)
        {
            // If the path isn't valid, return false.
            try
            {
                if (string.IsNullOrEmpty(path) || !Directory.Exists(Path.GetDirectoryName(path)))
                    return false;
            }

            // If it failed to even check if it was valid, that means it can't be used at all.
            catch { return false; }

            // Create a new project in the "CurrentProject", and write a new file there.
            Backend.MainAppViewModel.CurrentProject = new ProjectViewModel(DefaultProjectName, new ProjectFilesViewModel(new ObservableCollection<ProjectFileViewModel>(), new ObservableCollection<ProjectFileViewModel>()),
                "", "", DefaultCygwinStartUp, DefaultCygwinStop, DefaultBuildStart, DefaultBuildEachAssembly, DefaultBuildEachC, DefaultBuildFinish, DefaultTestCode);

            File.WriteAllText(path, "");

            // Save the project there now that there's a file.
            SaveProjectToPath(Backend.MainAppViewModel.CurrentProject.ToData(), path);

            // Set the path, and we're all done, leaving everything up to that.
            Backend.MainAppViewModel.ProjectPath = path;

            return true;
        }

        /// <summary>
        /// (Attempts to) load save data from the path given.
        /// </summary>
        /// <returns>Whether the project was valid.</returns>
        public static bool LoadProject(string path)
        {
            // If the path isn't valid, return false.
            if (!File.Exists(path))
                return false;

            // Read the path's contents
            var str = File.ReadAllText(path);

            // Use Newtonsoft.Json to actually load the project.
            var proj = Newtonsoft.Json.JsonConvert.DeserializeObject<Project>(str);

            // Convert the project into a ViewModel so that the rest of the app can understand it.
            Backend.MainAppViewModel.CurrentProject = ProjectViewModel.FromData(proj);
            return true;
        }

        /// <summary>
        /// (Attempts to) save current save data to the current path.
        /// </summary>
        /// <returns>Whether we able to save the project.</returns>
        public static bool SaveProject(Project save)
        {
            return SaveProjectToPath(save, Backend.MainAppViewModel.ProjectPath);
        }

        /// <summary>
        /// (Attempts to) save save data to the path given.
        /// </summary>
        /// <returns>Whether we able to save the project.</returns>
        public static bool SaveProjectToPath(Project save, string path)
        {
            // If the path isn't valid, return false.
            try
            {
                if (!File.Exists(path))
                    return false;
            }

            // If it failed to even check if it was valid, that means it can't be used at all.
            catch { return false; }

            // Use Newtonsoft.Json to serialize it.
            var str = Newtonsoft.Json.JsonConvert.SerializeObject(save);

            // If we made it here, actually save the project.
            File.WriteAllText(path, str);
            return true;
        }
    }
}
