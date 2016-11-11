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
            InitializeComponent();
            this._parent = _parent;
            this.client = client;
            this.group = group;
            this.appraiser = appraiser;
            this.appraisal = appraisal;

  
            var rep = new PresetRepository();
            IList<Preset> presets = new List<Preset>();
            presets = rep.FindAll();       
            presetBox.ItemsSource = presets;

            var catRep = new TestCategoryRepository();
            IList<TestCategory> cats = catRep.FindAll();

            IList<RowItem> items = new List<RowItem>();

            foreach (TestCategory cat in cats)
            {
                items.Add(new RowItem() { category = cat, categoryTests = getListBoxItems(cat) });
            }

            catsDataGrid.ItemsSource = items;
        }

        private void btn_Back(object sender, RoutedEventArgs e)
        {
            int childNumber = this._parent.stkTest.Children.Count;
            this._parent.stkTest.Children.RemoveAt(childNumber - 1);            
        }

        private void btn_AddTestsToPreset(object sender, RoutedEventArgs e)
        {
            IList<Test> tests = new List<Test>();
            foreach(RowItem item in catsDataGrid.Items)
            {
                IList<ListBoxItem> lbItems = item.categoryTests;
                foreach(ListBoxItem lbItem in lbItems)
                {
                    if (lbItem.isSelected)
                    {
                        tests.Add(lbItem.test);
                    }
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
            foreach (RowItem item in catsDataGrid.Items)
            {
                IList<ListBoxItem> lbItems = item.categoryTests;
                foreach(ListBoxItem lbItem in lbItems)
                {
                    if (lbItem.isSelected)
                    {
                        tests.Add(lbItem.test); //ADDS ALL TESTS!! even if they are already in the history view
                    }
                }
            }
            //TODO: if test is already added from selection dont add it from presets again

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
                

                //-----------------------delete later
                List<Appraisal_tests> at = new List<Appraisal_tests>();
                foreach (Test test in tests)
                {
                    Appraisal_tests one = new Appraisal_tests();
                    one.appraisalId = appraisal.id;
                    one.testId = test.id;
                    one.score = Decimal.Parse("0");
                    one.note = "Testnote";
                    at.Add(one);

                }
                repo.Insert(appraisal, appraiser, at);
                //-------------------

                // Going to the Main appraisal history view
                int childNumber = this._parent.stkTest.Children.Count;
                this._parent.stkTest.Children.RemoveRange(childNumber - 3, childNumber);

                CAH obj = new CAH(this._parent, client, group);

               // // Adding new column which header is appraisals date and what client can edit.
               // DataGridTextColumn textColumn = new DataGridTextColumn();
               // textColumn.Header = this.appraisal.date.ToString();
               //// textColumn.Binding = new Binding("FirstName");
                //obj.dataGrid.Columns.Add(textColumn);
                

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
            public bool isEnabled { get; set; }
        }

        class RowItem
        {
            public RowItem()
            {
            }
            public TestCategory category { get; set; }
            public IList<ListBoxItem> categoryTests { get; set; }
        }

        private IList<ListBoxItem> getListBoxItems(TestCategory cat) 
        {
            var repo = new TestRepository();
            IList<Test> tests = repo.GetTestsByCategory(cat);

            IList<ListBoxItem> items = new List<ListBoxItem>();
            foreach (Test t in tests)
            {
                    items.Add(new ListBoxItem() { test = t, isSelected = false, isEnabled=true });
             }
            return items;
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
