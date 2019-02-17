using ABParse;
using AssemblyDevelopmentApp.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyDevelopmentApp
{
    public enum BashScriptBlockParserTokens
    {
        NewLine,
        PROJPATH,
        PROJFILE,
        PROJNAME,
        FINALOUT,
        LINKFILE,
        FILETITLE,
        FILEIN,
        FILEOUT,
        ASMFILESIN,
        ASMFILESOUT,
        CFILESIN,
        CFILESOUT
    }

    /// <summary>
    /// Parsing a block of bash and converts any %PLACEHOLDERS% into the correct value - as well as newlines into " ; ".
    /// </summary>
    public class BashScriptBlockParser : ABParser
    {
        public Project Project;
        public string Result;
        public ProjectFile File;

        protected override void OnEnd()
        {
            // If there weren't any tokens at all, just make the final result the text.
            if (Result == "")
                Result = new string(Text);
        }

        public BashScriptBlockParser(Project project, ProjectFile file = null)
        {
            Project = project;
            File = file;

            Tokens = new System.Collections.ObjectModel.ObservableCollection<ABParserToken>()
            {
                new ABParserToken("NewLine", Environment.NewLine),
                new ABParserToken(nameof(BashScriptBlockParserTokens.PROJPATH), '%', 'P', 'R', 'O', 'J', 'P', 'A', 'T', 'H', '%'),
                new ABParserToken(nameof(BashScriptBlockParserTokens.PROJFILE), '%', 'P', 'R', 'O', 'J', 'F', 'I', 'L', 'E', '%'),
                new ABParserToken(nameof(BashScriptBlockParserTokens.PROJNAME), '%', 'P', 'R', 'O', 'J', 'N', 'A', 'M', 'E', '%'),
                new ABParserToken(nameof(BashScriptBlockParserTokens.FINALOUT), '%', 'F', 'I', 'N', 'A', 'L', 'O', 'U', 'T', '%'),
                new ABParserToken(nameof(BashScriptBlockParserTokens.LINKFILE), '%', 'L', 'I', 'N', 'K', 'F', 'I', 'L', 'E', '%'),
                new ABParserToken(nameof(BashScriptBlockParserTokens.FILETITLE), '%', 'F', 'I', 'L', 'E', 'T', 'I', 'T', 'L', 'E', '%'),
                new ABParserToken(nameof(BashScriptBlockParserTokens.FILEIN), '%', 'F', 'I', 'L', 'E', 'I', 'N', '%'),
                new ABParserToken(nameof(BashScriptBlockParserTokens.FILEOUT), '%', 'F', 'I', 'L', 'E', 'O', 'U', 'T', '%'),
                new ABParserToken(nameof(BashScriptBlockParserTokens.ASMFILESIN), '%', 'A', 'S', 'M', 'F', 'I', 'L', 'E', 'S', 'I', 'N', '%'),
                new ABParserToken(nameof(BashScriptBlockParserTokens.ASMFILESOUT), '%', 'A', 'S', 'M', 'F', 'I', 'L', 'E', 'S', 'O', 'U', 'T', '%'),
                new ABParserToken(nameof(BashScriptBlockParserTokens.CFILESIN), '%', 'C', 'F', 'I', 'L', 'E', 'S', 'I', 'N', '%'),
                new ABParserToken(nameof(BashScriptBlockParserTokens.CFILESOUT), '%', 'C', 'F', 'I', 'L', 'E', 'S', 'O', 'U', 'T', '%'),
            };

            IgnoreWhitespace = false;
        }

        protected override void OnTokenProcessed(TokenProcessedEventArgs e)
        {
            base.OnTokenProcessed(e);
            Result += e.Leading;
            switch (e.Token.Name)
            {
                case nameof(BashScriptBlockParserTokens.NewLine):
                    Result += " ; ";
                    break;
                case nameof(BashScriptBlockParserTokens.PROJPATH):
                    Result += Path.GetDirectoryName(Backend.MainAppViewModel.ProjectPath).Replace('\\', '/');
                    break;
                case nameof(BashScriptBlockParserTokens.PROJFILE):
                    Result += Backend.MainAppViewModel.ProjectPath.Replace('\\', '/');
                    break;
                case nameof(BashScriptBlockParserTokens.FINALOUT):
                    Result += Project.FinalOutput.Replace('\\', '/');
                    break;
                case nameof(BashScriptBlockParserTokens.LINKFILE):
                    Result += Project.LinkFile.Replace('\\', '/');
                    break;
                case nameof(BashScriptBlockParserTokens.PROJNAME):
                    Result += Project.ProjectName;
                    break;
                case nameof(BashScriptBlockParserTokens.FILETITLE):

                    if (File != null)
                        Result += File.FileTitle;

                    break;
                case nameof(BashScriptBlockParserTokens.FILEIN):

                    if (File != null)
                        Result += File.InputFilePath.Replace('\\', '/');

                    break;
                case nameof(BashScriptBlockParserTokens.FILEOUT):

                    if (File != null)
                        Result += File.OutputFilePath.Replace('\\', '/');

                    break;

                case nameof(BashScriptBlockParserTokens.ASMFILESIN):

                    // Go through each assembly file and add their inputs.
                    for (int i = 0; i < Project.Files.AssemblyFiles.Count; i++)
                        Result += Project.Files.AssemblyFiles[i].InputFilePath.Replace('\\', '/') + " ";

                    break;

                case nameof(BashScriptBlockParserTokens.ASMFILESOUT):

                    // Go through each assembly file and add their outputs.
                    for (int i = 0; i < Project.Files.AssemblyFiles.Count; i++)
                        Result += Project.Files.AssemblyFiles[i].OutputFilePath.Replace('\\', '/') + " ";

                    break;

                case nameof(BashScriptBlockParserTokens.CFILESIN):

                    // Go through each c file and add their inputs.
                    for (int i = 0; i < Project.Files.CFiles.Count; i++)
                        Result += Project.Files.CFiles[i].InputFilePath.Replace('\\', '/') + " ";

                    break;

                case nameof(BashScriptBlockParserTokens.CFILESOUT):

                    // Go through each c file and add their outputs.
                    for (int i = 0; i < Project.Files.CFiles.Count; i++)
                        Result += Project.Files.CFiles[i].OutputFilePath.Replace('\\', '/') + " ";

                    break;
            }
            if (e.NextToken == null)
                Result += e.Trailing;
        }
    }
}
