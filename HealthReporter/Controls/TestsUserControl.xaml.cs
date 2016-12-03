using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using HealthReporter.Models;
using System.Diagnostics;
using System.Windows.Media;
using System.Linq;
using System.Windows.Input;

namespace HealthReporter.Controls
{
    /// <summary>
    /// Interaction logic for UserControl2.xaml
    /// </summary>
    public partial class TestsUserControl : UserControl
    {
        private MainWindow _parent;
        private Models.TestCategory _category;
        private IList<TabItem> _tabitems; //to updated rating tabcontrol without database when adding/deleting is cancelled
        private int _selectedGroup = 0; //to select last tab which was selected before adding/deleting rating was cancelled

        public TestsUserControl(MainWindow parent)
        {          
            InitializeComponent();
            this._parent = parent;

            var catRep = new TestCategoryRepository();
            IList<TestCategory> categories = catRep.FindRootCategories();
            catsDataGrid.ItemsSource = categories;

            if (categories.Count > 0) catsDataGrid.SelectedIndex = 0;

            var repo = new TestRepository();
            IList<Test> tests = repo.FindAll();
            

            decimalsSelector.ItemsSource = new List<int> { -2, -1, 0, 1, 2 };

            btnShowTests.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#E0EEEE"));

            findTestTotal();
        }

        private void findTestTotal()
        {
            // Total client amount
            var repoT = new TestRepository();
            IList<Test> alltests = repoT.FindAll();

            testTotal.Text = alltests.Count.ToString() + " Tests";
        }

        private void btn_AddNewCategory(object sender, RoutedEventArgs e)
        {
            try
            {
                var category = new Models.TestCategory()
                {
                    name = "untitled category",
                };

                var repo2 = new TestCategoryRepository();
                repo2.Insert(category);
                this._category = category;
            }
            catch
            {
                MessageBox.Show("Something went wrong with adding a new category.");
            }

            // Updating Category menu
            var repo = new TestCategoryRepository();
            IList<Models.TestCategory> categories = repo.FindAll();

            // Add focus on new added row, put row into editable mode          
            int row = categories.Count - 1;
            catsDataGrid.Focus();
            catsDataGrid.ItemsSource = categories;
            catsDataGrid.SelectedIndex = row;
            catsDataGrid.CurrentCell = new DataGridCellInfo(catsDataGrid.Items[row], catsDataGrid.Columns[0]);
            catsDataGrid.IsReadOnly = false;
            catsDataGrid.BeginEdit();

            // Making tests grids empty
            testsDataGrid.ItemsSource = null;
            testDetailMain.DataContext = null;
        }

        private void GenderTabsItemssource(System.Collections.ObjectModel.ObservableCollection<TabItem> list)
        {
            for (int i = 0; i < MenAgesTab.Items.Count; i++) //fixes annoying binding warnings
            {
                System.Windows.Controls.TabItem c = (System.Windows.Controls.TabItem)MenAgesTab.ItemContainerGenerator.ContainerFromItem(MenAgesTab.Items[i]);
                if(c != null) c.Template = null;
            }
            for (int i = 0; i < WomenAgesTab.Items.Count; i++)
            {
                System.Windows.Controls.TabItem c = (System.Windows.Controls.TabItem)WomenAgesTab.ItemContainerGenerator.ContainerFromItem(WomenAgesTab.Items[i]);
                if(c != null) c.Template = null;
            }
            if (list != null && list.Count != 0)
            {
                list.Add(new TabItem() { interval = new AgeInterval() { interval = "+" }, rowitems = new List<RowItem>() });
                list.Add(new TabItem() { interval = new AgeInterval() { interval = "-" }, rowitems = new List<RowItem>() });
            }
            MenAgesTab.ItemsSource = list;
            WomenAgesTab.ItemsSource = list;
        }

        private void RatingsDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                RowItem item = e.Row.DataContext as RowItem;
                DataGrid grid = sender as DataGrid;

                int age = -1;
                if ((GenderTab.SelectedItem as System.Windows.Controls.TabItem).Header.ToString() == "Men") age = (int)MenAgesTab.SelectedValue;
                else age = (int)WomenAgesTab.SelectedValue;
                Test test = (Test)testName.DataContext;

                int itemIndex = grid.Items.IndexOf(item);

                if (item.normF == 0) item.normF = item.normM;
                else if (item.normM == 0) item.normM = item.normF;

                Rating rat = new Rating() { age = age, normF = item.normF, normM = item.normM, testId = test.id, labelId = item.LabelId };

                var ratingRep = new RatingRepository();
                var labelRep = new RatingLabelRepository();
                IList<Rating> ratings = ratingRep.getSameAgeRatings(new Rating() { testId = test.id, age = age });

                if (item.LabelId == null)
                {
                    byte[] id = System.Guid.NewGuid().ToByteArray();

                    RatingLabel label = new RatingLabel() { id = id, interpretation = item.interpretation, name = item.name };
                    if (item.rating == null) label.rating = 0;
                    else label.rating = colorToInt(item.rating.ToString());
                    rat.labelId = id;
                    labelRep.Insert(label);
                }
                else
                {
                    RatingLabel dbLabel = labelRep.getLabel(rat)[0];

                    int label_rating = 0;
                    if (item.rating != null) label_rating = colorToInt(item.rating.ToString());

                    if (dbLabel.name != item.name || dbLabel.rating != label_rating || dbLabel.interpretation != item.interpretation)
                    {
                        RatingLabel label = new RatingLabel() { id = dbLabel.id, interpretation = item.interpretation, name = item.name, rating=label_rating };
                        labelRep.Update(label);
                    }
                }
                var testRep = new TestRepository();
                if (ratings.Count > itemIndex)
                {
                    Rating dbRating = ratings[itemIndex];
                    ratingRep.Update(dbRating, rat);
                    testRep.Update(test);
                }
                else
                {
                    ratingRep.Insert(rat);
                    testRep.Update(test);
                }
            }
            validation2();
        }

        private int colorToInt(String color)
        {
            int rating = -1;

            switch (color.ToString())
            {
                case "#FFFF0000":
                    rating = 0;
                    break;
                case "#FFFFA500":
                    rating = 1;
                    break;
                case "#FFFFFF00":
                    rating = 2;
                    break;
                case "#FF008000":
                    rating = 3;
                    break;
                case "#FF0000FF":
                    rating = 4;
                    break;
                default: { break; }
            }
            return rating;
        }

        private void KeyDownHandler(object sender, KeyEventArgs e) //removes selected rating and ratingLabel from the database if Delete key is pressed
        {
            int tab = (int)WomenAgesTab.SelectedValue;
            if ((GenderTab.SelectedItem as System.Windows.Controls.TabItem).Header.ToString() == "Men")
            {
               tab = (int)MenAgesTab.SelectedValue;
            }
            var grid = (DataGrid)sender;
            if (Key.Delete == e.Key)
            {
                foreach (var row in grid.SelectedItems)
                {
                    if (row is RowItem)
                    {
                        int age = -1;
                        if ((GenderTab.SelectedItem as System.Windows.Controls.TabItem).Header.ToString() == "Men") age = (int)MenAgesTab.SelectedValue;
                        else age = (int)WomenAgesTab.SelectedValue;

                        RowItem item = (RowItem)row;
                        var repo = new RatingRepository();
                        repo.DeleteRating(new Rating() { testId = ((Test)testName.DataContext).id, age = age, labelId = item.LabelId, normF = item.normF, normM = item.normM });
                        var rep = new RatingLabelRepository();
                        rep.DeleteByRating(new Rating() { labelId = item.LabelId });
                    }
                }
                updateTestView((Test)testName.DataContext);

                MenAgesTab.SelectedValue = tab;

                var testRep = new TestRepository();
                testRep.Update((Test)testName.DataContext);
            }
        }

        private void btn_AddNewTest(object sender, RoutedEventArgs e)
        {
            if (!validation2()) return;
            Test newTest = new Test() { };
            newTest.id = System.Guid.NewGuid().ToByteArray();
            newTest.name = "No Name";
            newTest.weight = 1;
            if ((TestCategory)catsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("You have not created any categories yet");
                return;
            }
            newTest.categoryId = ((TestCategory)catsDataGrid.SelectedItem).id;

            //new test is inserted into db
            var repo = new TestRepository();
            repo.Insert(newTest);

            int i = catsDataGrid.SelectedIndex;
            catsDataGrid.SelectedIndex = -1;
            catsDataGrid.SelectedIndex = i;

            foreach(Test test in testsDataGrid.Items)
            {
                if (test.id.SequenceEqual(newTest.id)) testsDataGrid.SelectedItem = test;
            }
            findTestTotal();
            validation2();
        }

        private void catsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                if (catsDataGrid.SelectedItem != null)
                {
                    if (catsDataGrid.SelectedItem is Models.TestCategory)
                    {
                        var row = (Models.TestCategory) catsDataGrid.SelectedItem;
                        if (row != null)
                        {
                            var category = this._category;
                            category.name = (e.EditingElement as TextBox).Text;

                            var repo = new TestCategoryRepository();
                            repo.Update(category);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            catsDataGrid.IsReadOnly = true;
        }

        private void catsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) //is called when a catecory is selected
        {
            e.Handled = true;
            Models.TestCategory selectedCategory = (Models.TestCategory) catsDataGrid.SelectedItem;
            this._category = selectedCategory;

            var repo = new TestCategoryRepository();
            if (this._category != null)
            {
                search.Visibility = Visibility.Visible;
                updateTestsColumn(this._category);
            }
        }
        
        private void testsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) //is called when a test is selected
        {
            e.Handled = true;
            var grid = sender as DataGrid;
            var selected = grid.SelectedItems;

            if (selected.Count > 0)
            {
                Test test = (Test)selected[0];
                updateTestView(test);

                testDetailDatagrid.Visibility = Visibility.Visible;

                // Place a cursor to a testName textbox
                Keyboard.Focus(testName);
                testName.Select(testName.Text.Length, 0);
            }
            else testDetailDatagrid.Visibility = Visibility.Hidden;
        }

        private void updateTestsColumn(TestCategory cat) //cat is a main category
        {
            var rep = new TestCategoryRepository();

            //get subcategories
            IList<TestCategory> subCats = rep.GetCategoryByParent(cat);

            //get tests of subcategories
            var repo = new TestRepository();
            List<Test> cat_tests = new List<Test>();
            foreach (var category in subCats)
            {
                IList<Test> t = repo.GetTestsByCategory(category);
                cat_tests.AddRange(t);
            }
            //get tests of main category (if there are any)
            IList<Test> tsts = repo.GetTestsByCategory(cat);
            cat_tests.AddRange(tsts);

            testsDataGrid.ItemsSource = cat_tests;           
        }

        //updates test fields
        private void updateTestView(Test test)
        {
            testName.DataContext = test;
            units.DataContext = test;
            TestDescriptionText.DataContext = test;
            decimalsSelector.DataContext = test;
            weight.DataContext = test;
            FormulaTextF.DataContext = test;
            FormulaTextM.DataContext = test;

            IList<TabItem> tabitems = new List<TabItem>();
            IList<AgeInterval> ageIntervals = getAgeIntervals(test);
  
            IList<Rating> rats = new List<Rating>();

            var repo = new RatingRepository();
            foreach (AgeInterval ageint in ageIntervals)
            {
                List<RowItem> items = new List<RowItem>();
                IList<Rating> sameAgeRatings = repo.getSameAgeRatings(ageint.rating);

                var rep = new RatingLabelRepository();
                IList<RatingLabel> labs = new List<RatingLabel>();

                foreach (Rating rat in sameAgeRatings)
                {
                    labs = rep.getLabel(rat);
                    foreach (RatingLabel lab in labs)
                    {

                        Brush brush = ratingToColor(lab.rating);
                        items.Add(new RowItem() {name = lab.name, interpretation=lab.interpretation, LabelId=lab.id, normF=rat.normF, normM=rat.normM, rating=brush});
                    }
                    if (labs.Count == 0)
                    {
                        items.Add(new RowItem() { normF = rat.normF, normM = rat.normM, rating = ratingToColor(0)});
                    }
                }
                          
                tabitems.Add(new TabItem() { interval = ageint, rowitems = items});
            }
            GenderTabsItemssource(new System.Collections.ObjectModel.ObservableCollection<TabItem>(tabitems));
            this._tabitems = tabitems;

            if (tabitems.Count > 0)
            {
                MenAgesTab.SelectedIndex = 0;
                addRatingsM.IsEnabled = false;
                addRatingsF.IsEnabled = false;
                addRatingsM.Visibility = Visibility.Hidden;
                addRatingsF.Visibility = Visibility.Hidden;
            }
            else
            {
                addRatingsM.IsEnabled = true;
                addRatingsF.IsEnabled = true;
                addRatingsM.Visibility = Visibility.Visible;
                addRatingsF.Visibility = Visibility.Visible;
            }
            validation2();
        }

        private Brush ratingToColor(int rating)
        {
            string color = "Red";

            switch (rating)
            {
                case 0:
                    color = "Red";
                    break;
                case 1:
                    color = "Orange";
                    break;
                case 2:
                    color = "Yellow";
                    break;
                case 3:
                    color = "Green";
                    break;
                case 4:
                    color = "Blue";
                    break;
                default: { break; }
            }
            var converter = new BrushConverter();
            var brush = (Brush)converter.ConvertFromString(color);
            return brush;
        }

        private bool saveChangesToDb(Test test) //saves changes made on selected test to db, returns true if category was changed
        {
            if (test == null) return false;

            var repo = new TestRepository();
            IList<Test> tests = repo.Get(test);
            if (tests.Count == 0) return false;
            Test dbTest = tests[0];

            if (!test.categoryId.SequenceEqual(dbTest.categoryId) || test.decimals!=dbTest.decimals || test.description != dbTest.description || test.formulaF != dbTest.formulaF ||
                test.formulaM != dbTest.formulaM || test.name != dbTest.name || test.units != dbTest.units || test.weight != dbTest.weight || test.position != dbTest.position)
            {
                repo.Update(test);
            }
            return (!test.categoryId.SequenceEqual(dbTest.categoryId));
        }

        private IList<AgeInterval> getAgeIntervals(Test test) //gets ages connected with the test FROM the db, ageInterval{interval; rating}
        {
            var repo = new RatingRepository();
            IList<Rating> ages = repo.getAges(test);
            IList<AgeInterval> intervals = new List<AgeInterval>();

            for (int i = 0; i < ages.Count; i++)
            { 
                Rating rating = (Rating)ages[i];
                int age = rating.age;
                if (i + 1 < ages.Count)
                {
                    int nextAge = ((Rating)ages[i + 1]).age;
                    intervals.Add(new AgeInterval() { interval = age + "-" + (nextAge - 1), rating = rating });
                }
                else intervals.Add(new AgeInterval() { interval = age + "+", rating = rating });
            }
            return intervals;
        }

        private void btn_AddStuff(object sender, RoutedEventArgs e)
        {
            (sender as Button).ContextMenu.IsEnabled = true;
            (sender as Button).ContextMenu.PlacementTarget = (sender as Button);
            (sender as Button).ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            (sender as Button).ContextMenu.IsOpen = true;
        }

        private void btn_AddNewAge(object sender, RoutedEventArgs e) //adds new age group
        {
            if (testsDataGrid.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a test.");
                return;
            }

            int lastAge = 0;
            if (MenAgesTab.Items.Count > 0)
            {
                TabItem item = (TabItem)MenAgesTab.Items[MenAgesTab.Items.Count - 3];
                lastAge = item.interval.rating.age;
            }
            Test test = (Test)testName.DataContext;
            InputDialog inputDialog = new InputDialog("Please enter age group for which you would like to add ratings:", lastAge + "-");
            if (inputDialog.ShowDialog() == true)
            {
                int frst = parse_age(inputDialog.Answer, true, 0);
                int last = parse_age(inputDialog.Answer, true, 1);

                if (frst != -1 && last != -1)
                {
                    var repo = new RatingRepository();
                    if (MenAgesTab.Items.Count == 0)
                    {
                        repo.Insert(new Rating() { testId = test.id, age = frst });
                        repo.Insert(new Rating() { testId = test.id, age = last + 1 });
                    }
                    else if (((TabItem)MenAgesTab.Items[MenAgesTab.Items.Count - 3]).interval.rating.age > frst)
                    {
                        repo.Insert(new Rating() { testId = test.id, age = frst });
                        repo.Insert(new Rating() { testId = test.id, age = last + 1 });
                    }
                    else
                    {
                        repo.Insert(new Rating() { testId = test.id, age = last + 1 });
                    }
                }
                else MessageBox.Show("Invalid age group.");
                updateTestView(test);
                validation2();
            }
            else
            {
               GenderTabsItemssource(new System.Collections.ObjectModel.ObservableCollection<TabItem>(this._tabitems));
               MenAgesTab.SelectedIndex = _selectedGroup;
            }
            validation2();

        }

        private void btn_DeleteRating(object sender, RoutedEventArgs e) //deletes last rating
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the last age group? All ratings associated with that age group will be deleted.", "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                TabItem tabitem = (TabItem)MenAgesTab.Items[MenAgesTab.Items.Count - 3];
                int last = tabitem.interval.rating.age;

                var repo = new RatingRepository();
                repo.removeRatingsByAge((Test)testName.DataContext, last);

                updateTestView((Test)testName.DataContext);
                validation2();
            }
            else
            {
                GenderTabsItemssource(new System.Collections.ObjectModel.ObservableCollection<TabItem>(this._tabitems));
                MenAgesTab.SelectedIndex = _selectedGroup;
            }
        }

        private int parse_age(string ageStr, bool last, int i)
        {
            int age = -1;
            if (ageStr.Split('-').Length==2)
            {
                Int32.TryParse(ageStr.Split('-')[i], out age);
            }
            return age;
        }

        private void removeOldRatings(Test test, int age)
        {
            var repo = new RatingRepository();
            var rep = new RatingLabelRepository();
            IList<Rating> ages = repo.getAges(test);
            foreach (Rating a in ages)
            {
                //removes ratings with given testId and age & removes ratingslabels
                if (a.age == age)
                {
                    rep.DeleteByAge(test, age);
                    repo.removeRatingsByAge(test, age);
                    break;
                }
            }
        }

        private void btn_DeleteTest(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the selected test? All appraisal results related to the test will be deleted. The test will also be deleted from every preset that contains it.", "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            if(result == MessageBoxResult.Yes)
            {
                Test test = (Test)testName.DataContext;
                var testRepo = new TestRepository();
                testRepo.Delete(test);

                var labRepo = new RatingLabelRepository();
                labRepo.Delete(test);

                var ratRepo = new RatingRepository();
                ratRepo.Delete(test);

                int i = catsDataGrid.SelectedIndex;
                catsDataGrid.SelectedIndex = -1;
                catsDataGrid.SelectedIndex = i;

                ClearFields();
                GenderTabsItemssource(null);



                IList<Test> tests = testRepo.FindAll();
                findTestTotal();
                validation2();
            }
        }

        private void ClearFields()
        {
            testName.DataContext = null;
            units.DataContext = null;
            decimalsSelector.DataContext = null;
            weight.DataContext = null;
            TestDescriptionText.DataContext = null;
            FormulaTextF.DataContext = null;
            FormulaTextM.DataContext = null;           
        }

        class AgeInterval
        {
            public string interval { get; set; }
            public Rating rating { get; set; }
        }
        class TabItem
        {
            public TabItem()
            {
            }
            public AgeInterval interval { get; set; }
            public IList<RowItem> rowitems { get; set; }
        }

        class RowItem { 
            public RowItem()
            {
            }
            public decimal normF { get; set; }
            public decimal normM { get; set; }
            public byte[] LabelId { get; set; }
            public string name { get; set; }
            public string interpretation { get; set; }
            public Brush rating { get; set; }
        }

        private void btn_Clients(object sender, RoutedEventArgs e)
        {
            if (!validation2()) return;
            ClientUserControl obj = new ClientUserControl(_parent);
            _parent.stkTest.Children.Clear();
            _parent.stkTest.Children.Add(obj);
            btnShowClients.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFF0F0F0"));
            btnShowTests.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        private void btn_Tests(object sender, RoutedEventArgs e)
        {
            if (!validation2()) return;
            TestsUserControl obj = new TestsUserControl(_parent);
            _parent.stkTest.Children.Clear();
            _parent.stkTest.Children.Add(obj);
            btnShowTests.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFF0F0F0"));
            btnShowClients.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        private void filterSearchBox(object sender, TextChangedEventArgs e)
        {
            if (!validation2()) return;
            string searchBy = search.Text;

            TestCategory selectedcategory = (TestCategory)catsDataGrid.SelectedItem;
            
            var repo = new TestRepository();
            IList<Test> result = repo.FindSearchResult(searchBy, selectedcategory);
           
            testsDataGrid.ItemsSource = result;
        }

        private void TextBox_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            validation2();
        }

        private bool validation2()
        {
            if (testName.DataContext != null && (((Test)testName.DataContext).name == "" || ((Test)testName.DataContext).name == "No Name"  || ((Test)testName.DataContext).units == "" || ((Test)testName.DataContext).units == null) || !ratingsCheck())
            {
                ratingsCheck();
                catsDataGrid.IsEnabled = false;
                testsDataGrid.IsEnabled = false;
                search.IsEnabled = false;
                return false;
            }
            catsDataGrid.IsEnabled = true;
            testsDataGrid.IsEnabled = true;
            search.IsEnabled = true;
            return true;
        }

        private bool ratingsCheck()
        {
            if (MenAgesTab.Items.Count == 0)
            {
                tip.Visibility = Visibility.Hidden;
                // tip.ToolTip = "Click on the '+ Add ratings' button the add ratings.";
                ratingLabel.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#575C5C"));
                return true;
            }
            else
            {
                foreach (TabItem tab in _tabitems)
                {
                    if (tab.rowitems.Count < 2)
                    {
                        tip.Visibility = Visibility.Visible;
                        tip.ToolTip = "All age groups must have at least two ratings or no ratings at all.";
                        ratingLabel.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("red"));
                        return false;
                    }
                }
                ratingLabel.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#575C5C"));
                tip.Visibility = Visibility.Hidden;
                return true;
            }
        }

        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if(tb.Name == "testName" && ((Test)testName.DataContext).name == "")
            {
                tb.FontStyle = FontStyles.Italic;
                tb.Text = "No Name";
                tb.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#575C5C");
            }
            else if(tb.Name == "weight") //to allow user to enter doubles, we can't use OnPropertyChanged to update weight so it is updated here
            {
                try
                {
                    ((Test)testName.DataContext).weight = double.Parse(weight.Text, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch(FormatException)
                {
                    MessageBox.Show("Weight must be a number.");
                    return;
                }
            }
            var rep = new TestRepository();
            rep.Update((Test)testName.DataContext);
        }

        private void MenuItem_DeleteCategory(object sender, RoutedEventArgs e)
        {
            if (testsDataGrid.Items.Count > 0)
            {
                MessageBox.Show("Please add tests from this category to another category.");
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the selected category?", "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    TestCategory cat = (TestCategory)catsDataGrid.SelectedItem;
                    var rep = new TestCategoryRepository();
                    rep.Delete(cat);

                    catsDataGrid.ItemsSource = rep.FindRootCategories();
                }
            }
        }

        private void MenuItem_RenameCategory(object sender, RoutedEventArgs e)
        {
            // finding the item to rename
            var item = (MenuItem)sender;
            var contextMenu = (ContextMenu)item.Parent;
            var item2 = (DataGrid)contextMenu.PlacementTarget;
            var renameobj = (Models.TestCategory)item2.SelectedCells[0].Item;

            // adding focus on the rename obj row
            catsDataGrid.Focus();
            catsDataGrid.SelectedItem = renameobj;
            catsDataGrid.IsReadOnly = false;
            catsDataGrid.BeginEdit();
        }

        private void testName_GotFocus(object sender, RoutedEventArgs e)
        {
            if(testName.Text == "No Name")
            {
                testName.FontStyle = FontStyles.Normal;
                testName.Text = "";
                testName.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#000000");
            }
        }

        private void MenAgesTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tc = sender as TabControl;
            if (tc != null)
            {
                TabItem item = (TabItem)tc.SelectedItem;
                if (item != null && item.interval.interval == "+") {
                    if (e.RemovedItems.Count > 0) _selectedGroup = tc.Items.IndexOf(e.RemovedItems[0]);
                    btn_AddNewAge(null, null);

                }
                else if(item != null && item.interval.interval == "-")
                {
                    if (e.RemovedItems.Count > 0) _selectedGroup = tc.Items.IndexOf(e.RemovedItems[0]);
                    btn_DeleteRating(null, null);
                }
            }
        }

        private void ratingGridCombos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }

        private void menRatingsDatagrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }

        private void menRatingsDatagrid_GotFocus(object sender, RoutedEventArgs e)
        {
           DataGrid grid = sender as DataGrid;
           grid.BeginEdit();
        }

        private void MenAgesTab_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsUnderTabHeader(e.OriginalSource as DependencyObject))
                CommitTables(sender as TabControl);
        }

        private bool IsUnderTabHeader(DependencyObject control)
        {
            if (control is System.Windows.Controls.TabItem)
                return true;
            DependencyObject parent = VisualTreeHelper.GetParent(control);
            if (parent == null)
                return false;
            return IsUnderTabHeader(parent);
        }

        private void CommitTables(DependencyObject control)
        {
            if (control is DataGrid)
            {
                DataGrid grid = control as DataGrid;
                grid.CommitEdit(DataGridEditingUnit.Row, true);
                return;
            }
            int childrenCount = VisualTreeHelper.GetChildrenCount(control);
            for (int childIndex = 0; childIndex < childrenCount; childIndex++)
                CommitTables(VisualTreeHelper.GetChild(control, childIndex));
        }

        private void GenderTab_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsUnderTabHeader(e.OriginalSource as DependencyObject))
                CommitTables(sender as TabControl);
        }

        /*
         * Prevents the next row getting selected in catsDataGrid after a user presses ENTER when finished renaming a category.
         */
        private void catsDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var datagrid = (DataGrid)sender;
                if (datagrid != null)
                {
                    catsDataGrid.CommitEdit();
                    e.Handled = true;
                }
            }
        }
    }
}