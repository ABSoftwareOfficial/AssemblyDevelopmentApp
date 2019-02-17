using AssemblyDevelopmentApp.Pages;
using AssemblyDevelopmentApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyDevelopmentApp.ValueConverters
{
    public class ApplicationPageValueConverter : BaseValueConverter<ApplicationPageValueConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((AppPages)value)
            {
                case AppPages.Project: // Project
                    return new ProjectPage();
                    
                case AppPages.Files: // Files
                    return new FilesPage();

                case AppPages.Execute: // Execute
                    return new ExecutePage();

                case AppPages.Save: // Save
                    return new SavePage();

                case AppPages.Overwrite: // Overwrite
                    return new OverwritePage();

                case AppPages.ScriptingHelp: // Scripting Help
                    return new ScriptingHelpPage();

                // If we couldn't find a valid page, break here.
                default:
                    Debugger.Break();
                    return null;
            }
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
