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
        
        private List<Test> tests = new List<Test>();
     

        public NewAppraisalStep2Control(MainWindow _parent, Client client, Group group)
        {           
            InitializeComponent();
            this._parent = _parent;
            this.client = client;
            this.group = group;
           
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
            if (this._parent.stkTest.Children.Count > 4) //?
            {
                this._parent.stkTest.Children.RemoveRange(3, this._parent.stkTest.Children.Count);
            }
            else
            {
                int childNumber = this._parent.stkTest.Children.Count;
                this._parent.stkTest.Children.RemoveAt(childNumber - 1);
                this._parent.stkTest.Children[childNumber - 2].Focus();
            }
                  
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

            AddNewPresetControl obj = new AddNewPresetControl(this._parent, tests, client, group);
            this.Opacity = 0.3;
            this.IsEnabled = false;
            this._parent.stkTest.Children.Add(obj);
        }

        private void btn_OK(object sender, RoutedEventArgs e)
        {
            var repo = new AppraisalsRepository();

            //Getting dates if no tests haven't been added
            List<DateTime> dateList = repo.FindAllDates(client).ToList();
            dateList.Sort((x, y) => y.CompareTo(x));


            //Finding all appraisal dates of client
            List<string> dates = new List<string>();
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
                    foreach (DateTime elem in dateList)
                    {
                        if (dates.Count == 0)
                        {
                            if (lbItem.isSelected && lbItem.isEnabled && !dates.Contains(elem.ToString()))
                            {
                               // MessageBox.Show("here");
                                tests.Add(lbItem.test); //adds only tests that are not in history view already
                            }
                        }else
                        {
                            if (lbItem.isSelected && lbItem.isEnabled && dates.Contains(elem.ToString()))
                            {
                              //  MessageBox.Show("here");
                                tests.Add(lbItem.test); //adds only tests that are not in history view already
                            }
                        }
                       
                    }
                    
                }
            }

            //adds preset tests to tests if they are not already there and if they are not already in the history
            var rep = new PresetTestRepository();
            var testRep = new TestRepository();
            
            foreach (Preset preset in presetBox.SelectedItems)
            {
                IList<PresetTest> preTests = rep.FindPresetTests(preset);
                foreach (PresetTest preTest in preTests)
                {
                    bool isHist = false;
                    foreach (RowItem item in catsDataGrid.Items)
                    {
                        IList<ListBoxItem> lbItems = item.categoryTests;
                        foreach (ListBoxItem lbItem in lbItems)
                        {
                            if (!lbItem.isEnabled && lbItem.test.id.SequenceEqual(preTest.testId))
                            {
                                isHist = true;
                            }
                        }
                    }
                    foreach (Test test in this.tests)
                    {
                        if (preTest.testId.SequenceEqual(test.id)) isHist = true; //kui see test on testide hulgas juba
                    }
                    if (!isHist) tests.Add(testRep.GetTestByPresetTest(preTest)[0]);  
                }
            }
            var aprep = new AppraisalsRepository();
            IList<Test> historyTests = aprep.FindAppraisalTests(this.client);
            if (tests.Count < 1 && historyTests.Count < 1)
            {
                MessageBox.Show("Please select tests/presests.", "Message");
            }
            else
            {
                IList<Appraisal> allAppraisals = repo.FindAllAppraisals(client);

                foreach(Appraisal elem in allAppraisals)
                {
                    List<Appraisal_tests> at = new List<Appraisal_tests>();
                    foreach (Test test in tests)
                    {
                        Appraisal_tests one = new Appraisal_tests();
                        one.appraisalId = elem.id;
                        one.testId = test.id;
                        one.score = Decimal.Parse("0");
                        at.Add(one);

                    }
                    repo.Insert(at);
                }
         
                // Going to the Main appraisal history view
                int childNumber = this._parent.stkTest.Children.Count;
                this._parent.stkTest.Children.RemoveRange(childNumber - 2, childNumber);

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
                    tests.Add(testRep.GetTestByPresetTest(preTest)[0]);
                }
            }
            String str = "";
            foreach(Test test in tests)
            {
                str += test.name + " \n";
            }
            MessageBox.Show(str); 
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this);
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                btn_Back(null, null);
            }
            else if (e.Key == Key.Enter)
            {
                btn_OK(null, null);
            }
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
