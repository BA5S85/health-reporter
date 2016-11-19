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

            var testRep = new TestRepository();
            IList<Test> tests = testRep.FindAll();
            var aprep = new AppraisalsRepository();
            IList<Test> historyTests = aprep.FindAppraisalTests(this.client);

            IList<RowItem> items = new List<RowItem>();

            //getsListBoxItems
            foreach (TestCategory cat in cats)
            {
                IList<ListBoxItem> lbitems = new List<ListBoxItem>();
                foreach (Test t in tests)
                {
                    if (t.categoryId.SequenceEqual(cat.id))
                    {
                        bool isSelected = false;
                        foreach (Test hist in historyTests)
                        {
                            if (hist.name == t.name) isSelected = true;
                        }
                        lbitems.Add(new ListBoxItem() { test = t, isSelected = isSelected, isEnabled = !isSelected });
                    }
            }
                items.Add(new RowItem() { category = cat, categoryTests = lbitems});
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

            //Finding all appraisal dates of client
            List<string> dates = new List<string>();
            var repo = new AppraisalsRepository();
            IList<HistoryTableItem> history = repo.FindAll(client);

            foreach (HistoryTableItem item in history)
            {
                if (item.date != null && !dates.Contains(item.date.ToString()))
                {
                    dates.Add(item.date.ToString());
                }
            }

            foreach (RowItem item in catsDataGrid.Items)
            {
                IList<ListBoxItem> lbItems = item.categoryTests;
                foreach(ListBoxItem lbItem in lbItems)
                {
                    if (lbItem.isSelected && lbItem.isEnabled || lbItem.isSelected && !dates.Contains(appraisal.date))
                    {
                        tests.Add(lbItem.test); //adds only tests that are not in history view already
                    }
                }
            }

            //adds preset tests to tests if they are not already there and if they are not already in the history
            var rep = new PresetTestRepository();
            var testRep = new TestRepository();
            var aprep = new AppraisalsRepository();
            IList<Test> historyTests = aprep.FindAppraisalTests(this.client);
            foreach (Preset preset in presetBox.SelectedItems)
            {
                IList<PresetTest> preTests = rep.FindPresetTests(preset);
                foreach (PresetTest preTest in preTests)
                {
                    bool isHist = false;
                    foreach(Test histTest in historyTests)
                    {
                        if (preTest.testId.SequenceEqual(histTest.id)) isHist = true;
                    }
                    foreach (Test test in this.tests)
                    {
                        if (preTest.testId.SequenceEqual(test.id)) isHist = true;
                    }
                    if(!isHist) tests.Add(testRep.GetTestsByPresetTest(preTest)[0]);
                }
            }
            if (tests.Count < 1 && historyTests.Count < 1)
            {
                MessageBox.Show("Please select test/tests.", "Message");
            }
            else
            {
               
                List<Appraisal_tests> at = new List<Appraisal_tests>();
                foreach (Test test in tests)
                {
                    Appraisal_tests one = new Appraisal_tests();
                    one.appraisalId = appraisal.id;
                    one.testId = test.id;
                    one.score = Decimal.Parse("0");                   
                    at.Add(one);

                }
                repo.Insert(appraisal, appraiser, at);
              
                // Going to the Main appraisal history view
                int childNumber = this._parent.stkTest.Children.Count;
                this._parent.stkTest.Children.RemoveRange(childNumber - 3, childNumber);

                CAH obj = new CAH(this._parent, client, group);                

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
            var rep = new AppraisalsRepository();
            IList<Test> tests = repo.GetTestsByCategory(cat);
            IList<Test> historyTests = rep.FindAppraisalTests(this.client);

            IList<ListBoxItem> items = new List<ListBoxItem>();
            foreach (Test t in tests)
            {
                bool isSelected = false;
                foreach(Test hist in historyTests)
                {
                    if (hist.name == t.name) isSelected = true;
                }
                    items.Add(new ListBoxItem() { test = t, isSelected = true, isEnabled = !isSelected});
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
