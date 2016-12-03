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
using HealthReporter.Models;

namespace HealthReporter.Controls
{
    /// <summary>
    /// Interaction logic for AddNewPresetControl.xaml
    /// </summary>
    public partial class AddNewPresetControl : UserControl
    {
        public MainWindow _parent;
        public IList<Test> tests;

        private Client client;
        private Group group;

        public AddNewPresetControl(MainWindow parentWindow, IList<Test> tests, Client client, Group group)
        {
            InitializeComponent();
            this._parent = parentWindow;
            this.client = client;
            this.group = group;
          

            this.tests = tests;
        }

        private void btn_Back(object sender, RoutedEventArgs e)
        {
            int childNumber = this._parent.stkTest.Children.Count;
            this._parent.stkTest.Children.RemoveAt(childNumber - 1);
            this._parent.stkTest.Children[childNumber - 2].Opacity = 1;
            this._parent.stkTest.Children[childNumber - 2].IsEnabled = true;
        }

        private void btn_CreateNewPreset(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(name.Text))
            {
                nameLabel.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("Red"));
                return;
            }
            byte[] presetId = System.Guid.NewGuid().ToByteArray();
            IList<PresetTest> presetTests = new List<PresetTest>();
            foreach (Test test in this.tests)
            {
                presetTests.Add(new PresetTest() { presetId = presetId, testId = test.id });
            }

            var rep = new PresetTestRepository();
            foreach (PresetTest test in presetTests)
            {
                rep.Insert(test);
            }

            var repo = new PresetRepository();
            repo.Insert(new Preset() { id = presetId, name = name.Text });



            int childNumber = this._parent.stkTest.Children.Count;
            this._parent.stkTest.Children.RemoveAt(childNumber - 2);
            
            NewAppraisalStep2Control obj = new NewAppraisalStep2Control(this._parent, client, group);
            this._parent.stkTest.Children.Add(obj);

        }
    }
}
