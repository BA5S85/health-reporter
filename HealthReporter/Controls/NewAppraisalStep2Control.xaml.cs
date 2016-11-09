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
using System.Diagnostics;

namespace HealthReporter.Controls
{
    /// <summary>
    /// Interaction logic for CAH_SelectTestControl.xaml
    /// </summary>
    public partial class NewAppraisalStep2Control : UserControl
    {
        private Client client;
        private MainWindow _parent;
        private Group group;
        private Appraiser appraiser;
        private Appraisal appraisal;
        private List<Test> tests = new List<Test>();
     

        public NewAppraisalStep2Control(MainWindow _parent, Client client, Group group, Appraiser appraiser, Appraisal appraisal)
        {
            var repo = new TestRepository();
            IList<Test> tests = repo.FindAll();
            
            InitializeComponent();
            this._parent = _parent;
            this.client = client;
            this.group = group;
            this.appraiser = appraiser;
            this.appraisal = appraisal;

            showTests();

            var rep = new PresetRepository();
            IList<Preset> presets = new List<Preset>();
            presets = rep.FindAll();       
            presetBox.ItemsSource = presets;
        }



        private void btn_Back(object sender, RoutedEventArgs e)
        {
            int childNumber = this._parent.stkTest.Children.Count;
            this._parent.stkTest.Children.RemoveAt(childNumber - 1);            
        }

        private void btn_AddTestsToPreset(object sender, RoutedEventArgs e)
        {
            IList<Test> tests = new List<Test>();
            foreach(ListBoxItem item in listBox.Items)
            {            
                if (item.isSelected)
                {
                    tests.Add(item.test);
                }
            }
            if(tests.Count == 0)
            {
                MessageBox.Show("Please select tests");
                return;
            }

            AddNewPresetControl obj = new AddNewPresetControl(this._parent, tests, client, group, appraiser, appraisal);
            this.Opacity = 0.3;
            this._parent.stkTest.Children.Add(obj);
        }

        private void btn_OK(object sender, RoutedEventArgs e)
        {
            foreach (var item in listBox.SelectedItems)
            {
                ListBoxItem lbItem = (ListBoxItem)item;
                Test test = lbItem.test;
                tests.Add(test);
            }
            //adds preset tests to tests
            var rep = new PresetTestRepository();
            var testRep = new TestRepository();
            foreach (Preset preset in presetBox.SelectedItems)
            {
                IList<PresetTest> preTests = rep.FindPresetTests(preset);
                foreach (PresetTest preTest in preTests)
                {
                    tests.Add(testRep.GetTestsByPresetTest(preTest)[0]);
                }
            }

            if (tests.Count < 1)
            {
                MessageBox.Show("Please select test/tests.", "Message");
            }
            else
            {
                // Adding appraiser and appraisal info to the database.
                AppraisalsRepository repo = new AppraisalsRepository();
                repo.Insert(appraisal, appraiser);

                // Going to the Main appraisal history view
                int childNumber = this._parent.stkTest.Children.Count;
                this._parent.stkTest.Children.RemoveRange(childNumber - 3, childNumber);

                CAH obj = new CAH(this._parent, client, group);

                // Adding new column which header is appraisals date and what client can edit.
                DataGridTextColumn textColumn = new DataGridTextColumn();
                textColumn.Header = this.appraisal.date.ToString();
               // textColumn.Binding = new Binding("FirstName");
                obj.dataGrid.Columns.Add(textColumn);
                

                this._parent.stkTest.Children.Add(obj);
            }
        }

        class ListBoxItem
        {
            public ListBoxItem()
            {
            }
            public Test test { get; set; }
            public bool isSelected { get; set; }
        }

        private void showTests() 
        {
            var repo = new TestRepository();

            IList<ListBoxItem> items = new List<ListBoxItem>();

            IList<Test> tests = repo.FindAll();
            int i = 0;
            foreach (Test t in tests)
            {
                    bool isA = i%2 == 0;
                    i++;
                    items.Add(new ListBoxItem() { test = t, isSelected = false });
             }
            listBox.ItemsSource = items;
        }

        private void deletePresetButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the selected presets?", "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                var rep = new PresetTestRepository();
                var repo = new PresetRepository();
                foreach(Preset preset in presetBox.SelectedItems)
                {
                    rep.Delete(preset);
                    repo.Delete(preset);
                }

                IList<Preset> presets = new List<Preset>();
                presets = repo.FindAll();
                presetBox.ItemsSource = presets;
            }
        }

        private void viewTests_Click(object sender, RoutedEventArgs e)
        {
            var rep = new PresetTestRepository();
            var testRep = new TestRepository();
            List<Test> tests = new List<Test>();
            foreach (Preset preset in presetBox.SelectedItems)
            {
                IList<PresetTest> preTests = rep.FindPresetTests(preset);
                foreach (PresetTest preTest in preTests)
                {
                    tests.Add(testRep.GetTestsByPresetTest(preTest)[0]);
                }
            }
            String str = "";
            foreach(Test test in tests)
            {
                str += test.name + " \n";
            }
            MessageBox.Show(str);

        }
    }

    public class InvertBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool booleanValue = (bool)value;
            return !booleanValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter,System.Globalization. CultureInfo culture)
        {
            bool booleanValue = (bool)value;
            return !booleanValue;
        }
    }
}
