using AssemblyDevelopmentApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AssemblyDevelopmentApp.Pages
{
    /// <summary>
    /// Interaction logic for ProjectPage.xaml
    /// </summary>
    public partial class ProjectPage : Page
    {
        public ProjectPage()
        {
            InitializeComponent();
            DataContext = Backend.CurrentPageViewModel;
        }

        private void EnterTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Below is some horrible WPF code so that
            // when we press the ENTER key on the project path, it will update its binding.
            if (e.Key == Key.Return)
            {
                // Get the DependencyProperty.
                var prop = TextBox.TextProperty;

                // Now, get the binding and update it if possible.
                BindingExpression be = BindingOperations.GetBindingExpression(ProjPath, prop);
                if (be != null)
                    be.UpdateSource();
            }
        }
    }
}
