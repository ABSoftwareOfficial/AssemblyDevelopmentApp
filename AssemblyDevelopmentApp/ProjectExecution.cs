using AssemblyDevelopmentApp.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AssemblyDevelopmentApp
{ 
    public delegate void OutputFromCygwinEventHandler(OutputFromCygwinEventArgs e);

    public class OutputFromCygwinEventArgs : EventArgs
    {
        public string NewText { get; set; }

        public OutputFromCygwinEventArgs(string newText)
        {
            NewText = newText;
        }
    }

    public static class ProjectExecution
    {
        // These will all wait for cygwin to do each.
        private static ManualResetEvent WaitForOutput;
        private static ManualResetEvent WaitForError;
        private static ManualResetEvent WaitForExit;

        public const string CygwinBashFile = @"cygwin\bin\bash.exe";

        /// <summary>
        /// The process that cygwin runs under.
        /// </summary>
        public static Process CygwinProcess { get; set; }

        /// <summary>
        /// The main output from cygwin.
        /// </summary>
        public static string Output { get; set; }

        #region Main

        ///// <summary>
        ///// Starts cygwin.
        ///// </summary>
        //public static void StartCygwin(Project project)
        //{
        //    // Starting cygwin means that cygwin will now start, and we won't be waiting for a response.
        //    CygwinRunning = true;
        //    WaitingForResponse = false;

        //    // Create a new QueuedCommands.
        //    QueuedCommands = new Queue<string>();

        //    // Initialize a new cygwin process.
        //    CreateCygwinProcess();

        //    // Start the actual cygwin process!
        //    CygwinProcess.Start();

        //    // Finally, execute the code to execute when starting up cygwin.
        //    ExecuteSingleCommand(project.CygwinStartUp);
        //}


        /// <summary>
        /// Builds the project.
        /// </summary>
        public static async Task<string> BuildProject(Project project)
        {
            var commands = CreateCommandArguments(WrapBashWithStartAndEnd(GetBuildBash(project), project), project);
            return await ExecuteCygwin(commands);
        }

        /// <summary>
        /// Tests the project.
        /// </summary>
        public static async Task<string> TestProject(Project project)
        {
            var commands = CreateCommandArguments(WrapBashWithStartAndEnd(project.TestCode, project), project);
            return await ExecuteCygwin(commands);
        }

        /// <summary>
        /// Builds and tests the project.
        /// </summary>
        public static async Task<string> BuildAndTestProject(Project project)
        {
            var commands = CreateCommandArguments(WrapBashWithStartAndEnd(GetBuildBash(project) + Environment.NewLine + project.TestCode, project), project);
            return await ExecuteCygwin(commands);
        }

        #endregion

        /// <summary>
        /// Creates all the bash that will need to be executed to build.
        /// </summary>
        private static string GetBuildBash(Project project)
        {
            var result = project.BuildStart;

            // Add all of the correct bash for each assembly file.
            for (int i = 0; i < project.Files.AssemblyFiles.Count; i++)
                result = GetFileBash(project, result, project.Files.AssemblyFiles[i], project.BuildEachAssembly);

            // Add all of the correct bash for each C file as well.
            for (int i = 0; i < project.Files.CFiles.Count; i++)
                result = GetFileBash(project, result, project.Files.CFiles[i], project.BuildEachC);

            // Add the build finish part and return the final result.
            if (project.BuildFinish != "")
                result += Environment.NewLine + project.BuildFinish;

            return result;
        }

        private static string GetFileBash(Project project, string result, ProjectFile file, string buildEach)
        {
            var parser = new BashScriptBlockParser(project, file);

            // Process this file so that any placeholder (such as FILEIN) will get handled.
            parser.Start(buildEach);
            return result += Environment.NewLine + parser.Result;
        }

        /// <summary>
        /// Executes cygwin, with all the commands given, and gets back the response.
        /// </summary>
        private static async Task<string> ExecuteCygwin(string commands)
        {
            // Reset the output and error output and create the base process.
            Output = null;
            CreateCygwinProcess();

            // Prepare all the "WaitEventHandle"s and events.
            PrepareEvents();
            PrepareWaitHandles();

            // Insert the commands onto the base process, and start the process.
            CygwinProcess.StartInfo.Arguments += "\"" + commands + "\"";
            CygwinProcess.Start();

            // Make the process starting outputting the output and error.
            CygwinProcess.BeginErrorReadLine();
            CygwinProcess.BeginOutputReadLine();

            // Wait until it's finished.
            await WaitUntilFinished();

            // Return the final output (main output + error output).
            return Output;
        }

        private static Task WaitUntilFinished()
        {
            // Wait for an output, error AND for it to exit.
            return Task.Run(() =>
            {
                while (!WaitHandle.WaitAll(new WaitHandle[] { WaitForOutput, WaitForError, WaitForExit }, 100))
                    continue;
            });
        }

        private static void CreateCygwinProcess()
        {
            // Create a process that cygwin will go in.
            var process = new Process();

            // Make sure it points to cygwin.
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            process.StartInfo.FileName = CygwinBashFile;

            // Give it the correct arguments (which bash needs to just start up).
            process.StartInfo.Arguments = "--login -c ";

            // Make sure it will redirect the outputs and errors.
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            // Don't allow error dialogs, and we don't want it using shell execute.
            process.StartInfo.ErrorDialog = false;
            process.StartInfo.UseShellExecute = false;

            // Make sure it will use the events, and that it doesn't open a window.
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;

            // Now, make it the main process.
            CygwinProcess = process;
        }

        /// <summary>
        /// Turns a block of bash (with %PLACEHOLDERS%) into inline arguments, ready to execute.
        /// </summary>
        /// <param name="commands">The commands to inline execute.</param>
        /// <returns></returns>
        private static string CreateCommandArguments(string commands, Project project)
        {
            // Process everything here with the BashScriptBlock, powered by ABParse.
            var parser = new BashScriptBlockParser(project);

            // Start it and return it.
            parser.Start(commands);
            return parser.Result.Replace('\\', '/').Replace(" ;  ; ", " ; ").Replace(" ;  ; ", " ; ").TrimStart(' ', ';', ' ');

        }

        private static void OnOutputReceived(string data)
        {
            if (data == null)
                WaitForOutput.Set();
            else
                Output += data + Environment.NewLine;
        }

        private static void OnErrorReceived(string data)
        {
            if (data == null)
                WaitForError.Set();
            else
                Output += data + Environment.NewLine;
        }

        private static void OnExited()
        {
            WaitForExit.Set();
        }

        private static void PrepareEvents()
        {
            CygwinProcess.OutputDataReceived += (s, e) => OnOutputReceived(e.Data);
            CygwinProcess.ErrorDataReceived += (s, e) => OnErrorReceived(e.Data);
            CygwinProcess.Exited += (s, e) => OnExited();
        }

        private static void PrepareWaitHandles()
        {
            WaitForError = new ManualResetEvent(false);
            WaitForExit = new ManualResetEvent(false);
            WaitForOutput = new ManualResetEvent(false);
        }
        
        private static string WrapBashWithStartAndEnd(string contents, Project project)
        {
            return project.CygwinStartUp + Environment.NewLine + contents + Environment.NewLine + project.CygwinStop;
        }

        

        //private static void InitializeInputOutput()
        //{
        //    // Set up the output and error data to go into the "OnOutputRecieved".
        //    CygwinProcess.OutputDataReceived += (s, e) => await HandleResponse();
        //    CygwinProcess.ErrorDataReceived += (s, e) => HandleResponse();
        //}

        //private static async void ExecuteQueuedCommands()
        //{
        //    // If we're already executing the queued up commands, don't do anything.
        //    if (ExecutingQueuedCommands)
        //        return;

        //    // We are now executing the queued commands.
        //    ExecutingQueuedCommands = true;

        //    // Otherwise, go through the queued commands until there are none left.
        //    await Task.Run(async () =>
        //    {
        //        while (QueuedCommands.Count != 0)
        //            await ExecuteNextCommand();
        //    });

        //    // We are no longer executing the queued commands.
        //    ExecutingQueuedCommands = false;
        //}

        ///// <summary>
        ///// Attempts to execute the next queued up command.
        ///// </summary>
        //private static async Task ExecuteNextCommand()
        //{
        //    // Don't do anything if there isn't a next command, or if we're waiting for the current command to finish.
        //    if (QueuedCommands.Count == 0 && WaitingForResponse)
        //        return;

        //    // Otherwise, execute the low-level part, which will send the command and emulate the RETURN key (yeah, P/Invoke is no fun)
        //    await SendCommandToCygwinAndRun(QueuedCommands.Dequeue());

        //    // Handle the response from that.
        //    await HandleResponse();
        //}

        //private static async Task HandleResponse()
        //{
        //    // Get the output and call the event for that.
        //    OutputFromCygwin(new OutputFromCygwinEventArgs(await CygwinProcess.StandardOutput.ReadToEndAsync()));

        //    // Get the error and call the event for that.
        //    OutputFromCygwin(new OutputFromCygwinEventArgs(await CygwinProcess.StandardError.ReadToEndAsync()));
        //}

        //private static async Task SendCommandToCygwinAndRun(string command)
        //{
        //    // Send the actual command to cygwin.
        //    await CygwinProcess.StandardInput.WriteAsync(command);
        //    await CygwinProcess.StandardInput.FlushAsync();

        //    // Close the input, since we've written what we want.
        //    CygwinProcess.StandardInput.Close();
        //}

    }
}
