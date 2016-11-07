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
using System.Windows.Shapes;

namespace HealthReporter.Controls
{
    /// <summary>
    /// Interaction logic for NewAppraisalStep1Control.xaml
    /// </summary>
    public partial class NewAppraisalStep1Control : UserControl
    {
        public MainWindow _parent;

        public NewAppraisalStep1Control(MainWindow parentWindow)
        {
            InitializeComponent();
            this._parent = parentWindow;
        }

        private void btn_Back(object sender, RoutedEventArgs e)
        {
            int childNumber = this._parent.stkTest.Children.Count;
            this._parent.stkTest.Children.RemoveAt(childNumber - 1);
        }
    }
}
