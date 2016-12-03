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
using System.Data;
using System.Globalization;
using System.Windows.Controls.Primitives;

namespace HealthReporter.Controls
{
    /// <summary>
    /// Interaction logic for ClientAppraisalHistoryControl.xaml
    /// </summary>
    public partial class CAH : UserControl
    {
        private Client client;
        private Group group;
        private MainWindow _parent;
        private IList<string> dates;


        public CAH(MainWindow _parent)
        {
            this._parent = _parent;
        }

        public CAH(MainWindow _parent, Client client, Group group) : this(_parent)
        {

            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            InitializeComponent();
            this._parent = _parent;
            this.client = client;
            this.group = group;

            Client client1 = this.client;

            if (client1.gender == "0")
            {
                ClientInfo.Text = client1.firstName + " " + client1.lastName + ", " + client1.age + ", Female";
            } else if (client1.gender == "1")
            {
                ClientInfo.Text = client1.firstName + " " + client1.lastName + ", " + client1.age + ", Male";
            }
            ClientGroup.DataContext = group;

            

            //Finding all appraisal dates of client
            List<string> dates = new List<string>();

            var latestAppraisalTests = new Dictionary<byte[], HistoryTableItem>(new ByteArrayComparer());

            //Finding all appraisal results of client
            var repo = new AppraisalsRepository();

            IList<HistoryTableItem> history = repo.FindAll(client);

            if (history.Count == 0)
            {
                //Getting dates into the CAH main view, if no tests haven't been added
                List<DateTime> dateList = repo.FindAllDates(client).ToList();
                dateList.Sort((x, y) =>y.CompareTo(x));

                foreach (DateTime elem in dateList)
                {
                    DataGridTextColumn textColumn = new DataGridTextColumn();
                    textColumn.Header = String.Format("{0:dd/MM/yyyy}",elem);                                                   
                    dataGrid.Columns.Add(textColumn);
                  
                }
            } 

            foreach (HistoryTableItem item in history)
            {
                if (item.date != null && !dates.Contains(item.date.ToString()))
                {
                    dates.Add(item.date.ToString());
                }

                //finding latest appraisal date for each test in appraisal history
                if (item.date != null && item.Score != 0)
                {
                    DateTime date = DateTime.Parse(item.date);

                    HistoryTableItem histItem;
                    if (latestAppraisalTests.TryGetValue(item.tId, out histItem)) //map contains testId
                    {
                        if (date > DateTime.Parse(histItem.date))
                        {
                            latestAppraisalTests[item.tId] = item;
                        }
                    }
                    else
                    {
                        latestAppraisalTests.Add(item.tId, item);
                    }
                }
            }

            dates.Sort((x, y) => DateTime.Parse(y).CompareTo(DateTime.Parse(x)));
            this.dates = dates;

            //Initializing datagrid objects
            List<FullHistoryDatagrid> result = findFullHistory(history);

            //Reading elements into table
            int i = 0;
            foreach (string elem in dates)
            {
                DataGridTextColumn textColumn = new DataGridTextColumn();
                textColumn.Header = String.Format("{0:dd/MM/yyyy}", DateTime.Parse(elem));
                Binding binding = new Binding("list[" + i + "]");
               
                binding.UpdateSourceTrigger = UpdateSourceTrigger.LostFocus;
                  
                textColumn.Binding = binding;

                Style style = new Style(typeof(DataGridCell))
                {
                    Setters = {
                                new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right)
                }
                };
                textColumn.CellStyle = style;

                dataGrid.Columns.Add(textColumn);
                i++;
            }

            dataGrid.ItemsSource = result;

            //categories datagrid
            catsDataGrid.ItemsSource = findCatsDataGridItems(latestAppraisalTests);

            begin.Visibility = Visibility.Visible;

        }

        private IList<CatsItem> findCatsDataGridItems(Dictionary<byte[], HistoryTableItem> latestMap)
        {
            Dictionary<byte[], decimal> categoryScores = new Dictionary<byte[], decimal>(new ByteArrayComparer());
            Dictionary<byte[], decimal> categoryMaxScores = new Dictionary<byte[], decimal>(new ByteArrayComparer());

            IList<CatsItem> items = new List<CatsItem>();

            var repo = new RatingRepository();

            foreach (byte[] id in latestMap.Keys)
            {
                int age = findAge(int.Parse(this.client.age), id);
                if (age != -1) //if age is -1 then test has no ratings and it can't be used in calculations
                {
                    IList<RatingMeaning> allRatingMeanings = repo.findHistoryTestRatings(age, id);
                    decimal score = latestMap[id].Score;
                    RatingMeaning scoreMeaning = findScoreMeaning(score, allRatingMeanings);
                    int scoreRating = scoreMeaning.rating;
                    int maxScoreRating = findMaxScoreRating(allRatingMeanings);

                    byte[] catId = latestMap[id].tCategory;

                    decimal weight = (decimal)latestMap[id].weight;

                    decimal catScore;
                    if (categoryScores.TryGetValue(catId, out catScore))
                    {
                        categoryScores[catId] = catScore + scoreRating * weight;
                        categoryMaxScores[catId] = categoryMaxScores[catId] + maxScoreRating * weight;
                    }
                    else
                    {
                        categoryScores.Add(catId, scoreRating * weight);
                        categoryMaxScores.Add(catId, maxScoreRating * weight);
                    }
                }
            }

            var rep = new TestCategoryRepository();
            IList<TestCategory> cats = rep.FindAll();

            //find percentages
            foreach (byte[] id in categoryScores.Keys)
            {
                decimal actual = categoryScores[id];
                decimal max = categoryMaxScores[id];

                decimal percentage = 0;
                if (max != 0)
                {
                    percentage = (actual * 100) / max;
                }
                else
                {
                    if (actual == 0) percentage = 100;
                }
                var brush = findColorCode(percentage);

                    foreach (TestCategory cat in cats)
                    {
                        if (cat.id.SequenceEqual(id))
                        {
                            items.Add(new CatsItem() { category = cat, color = brush, percentage = percentage.ToString() + "%" });
                        }
                    }
            }
            foreach(TestCategory cat in cats)
            {
                var converter = new BrushConverter();
                var brush = (Brush)converter.ConvertFromString("white");
                bool hasScore = false;
                foreach(byte[] id in categoryScores.Keys)
                {
                    if (cat.id.SequenceEqual(id)) hasScore = true;
                }
                if(!hasScore) items.Add(new CatsItem() { category = cat, color = brush, percentage = "-" });
            }

                return items;
        }

        private Brush findColorCode(decimal percentage)
        {
            string color = "Red";

            if (percentage <= 20) color = "Red";
            else if (percentage <= 40) color = "Orange";
            else if (percentage <= 60) color = "Yellow";
            else if (percentage <= 80) color = "Green";
            else color = "Blue";

            var converter = new BrushConverter();
            var brush = (Brush)converter.ConvertFromString(color);
            return brush;
        }

        private int findAge(int clientAge, byte[] id)
        {
            var repo = new RatingRepository();
            IList<Rating> ratings = repo.getTestRatings(new Test() { id=id });
            if (ratings.Count == 0) return -1;
            int age = ratings[0].age;
            foreach(Rating rat in ratings)
            {
                if(clientAge > rat.age)
                {
                    age = rat.age;
                }
            }
            return age;
        }

        private RatingMeaning findScoreMeaning(decimal score, IList<RatingMeaning> list)
        {
            bool isMale = this.client.gender == "1";
            if (list.Count == 0) return new RatingMeaning() { rating = 0};
            RatingMeaning meaning = list[0];
            foreach(RatingMeaning mean in list)
            {
                var norm = mean.normF;
                if (isMale)
                {
                    norm = mean.normM;
                }

                if(score > norm)
                {
                    meaning = mean;
                }
            }
            return meaning;
        }

        private int findMaxScoreRating(IList<RatingMeaning> list)
        {
            if (list.Count == 0) return 0;
            int max = list[0].rating;
            foreach (RatingMeaning mean in list)
            {
                if (mean.rating > max)
                {
                    max = mean.rating;
                }
            }
            return max;
        }

        private Dictionary<byte[], HistoryTableItem> findLatestAppraisals(IList<HistoryTableItem> history)
        {
            var latestAppraisalTests = new Dictionary<byte[], HistoryTableItem>(new ByteArrayComparer());

            foreach (HistoryTableItem item in history)
            {
                //finding latest appraisal date for each test in appraisal history
                if (item.date != null && item.Score != 0)
                {
                    DateTime date = DateTime.Parse(item.date);

                    HistoryTableItem histItem;
                    if (latestAppraisalTests.TryGetValue(item.tId, out histItem)) //map contains testId
                    {
                        if (date > DateTime.Parse(histItem.date))
                        {
                            latestAppraisalTests[item.tId] = item;
                        }
                    }
                    else
                    {
                        latestAppraisalTests.Add(item.tId, item);
                    }
                }
            }
            return latestAppraisalTests;
        }

        private void btn_Back(object sender, RoutedEventArgs e)
        {
            int childNumber = this._parent.stkTest.Children.Count;
            this._parent.stkTest.Children.RemoveAt(childNumber - 1);
            this._parent.stkTest.Children[childNumber - 2].Opacity = 1;
            this._parent.stkTest.Children[childNumber - 2].IsEnabled = true;
        }

        private void btn_NewAppraisal(object sender, RoutedEventArgs e)
        {
            NewAppraisalStep1Control obj = new NewAppraisalStep1Control(this._parent, client, group);
            this._parent.stkTest.Children.Add(obj);
        }

        private void btn_AddTest(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataGrid.Columns[2].Header != null)
                {
                    NewAppraisalStep2Control obj = new NewAppraisalStep2Control(this._parent, client, group);
                    this._parent.stkTest.Children.Add(obj);
                }

            }
            catch
            {
                MessageBox.Show("Please add appraisal first.");
            }
            
           
           
        }

        private void btn_Report(object sender, RoutedEventArgs e)
        {
        }

        private void dataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            e.Cancel = true;
        }

        private void dataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            dataGrid.CommitEdit();
        }
        private void dataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {

            var editedTextbox = e.EditingElement as TextBox;
            FullHistoryDatagrid elem = (FullHistoryDatagrid)dataGrid.SelectedCells[0].Item;

            DataGridColumn col1 = e.Column;
            int index = col1.DisplayIndex;

            Date_Score_Appraiser elem2 = elem.list[index - 2];
            var repoAT = new Appraisal_tests_repository();

            IList<Appraisal_tests> history = repoAT.FindAll();

            Appraisal_tests appTest = new Appraisal_tests();
            appTest.testId = elem2.tId;
            appTest.appraisalId = elem2.applId;
            appTest.score = elem2.score;           
            
           
            var repo = new Appraisal_tests_repository();

            if (editedTextbox.Text.ToString() != "")
            {
               
                if (elem2.applId != null)
                {
                    try
                    {
                        System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
                        customCulture.NumberFormat.NumberDecimalSeparator = ".";
                        System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

                        decimal test1 = decimal.Parse(editedTextbox.Text);
                        appTest.score = test1;
                        
                        repoAT.Update(appTest);
                        
                    }
                    catch
                    {
                    }
                }
                else
                {
                    foreach (FullHistoryDatagrid item in dataGrid.Items)
                    {
                        foreach (Date_Score_Appraiser item2 in item.list)
                        {
                            if (item2.date == elem2.date && item2.applId != null)
                            {
                                appTest.appraisalId = item2.applId;
                                appTest.score = Decimal.Parse(editedTextbox.Text);
                                
                                repoAT.Insert(appTest);
                                return;
                            }
                        }
                    }
                }
               
            }
            var rep = new AppraisalsRepository();
            IList<HistoryTableItem> hist = rep.FindAll(client);
            var latestAppraisalTests = findLatestAppraisals(hist);
            catsDataGrid.ItemsSource = findCatsDataGridItems(latestAppraisalTests);
        }

        private int rowIndex = 0;
        private int columnIndex = 0;


        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rowIndex = dataGrid.SelectedIndex;

            if (columnIndex > -1)
            {
                // Finding the selected cell object
                FullHistoryDatagrid selectedItem = (FullHistoryDatagrid)dataGrid.SelectedItem;
                Date_Score_Appraiser elem = selectedItem.list[columnIndex];

                // Finding client age range
                RatingRepository repo = new RatingRepository();
                List<Rating> ratingsByTestId = repo.getRatingsByTestId(selectedItem).ToList();

                var ageslist = new SortedSet<int>();

                int min = 0;
                int max = 0;

                foreach (Rating rating in ratingsByTestId)
                {
                    ageslist.Add(rating.age);
                }

                foreach (int setElem in ageslist)
                {

                    if (setElem < int.Parse(client.age))
                    {
                        min = setElem;

                    }
                    else
                    {
                        max = setElem;
                        break;
                    }
                }
                if (max <= 0)
                {
                    ageslabel.Text = "ages " + min.ToString() + "+";
                }
                else
                {
                    ageslabel.Text = "ages " + min.ToString() + "-" + (max - 1).ToString();
                }

                // Finding rating labels with meanings
                stackpanel.Children.Clear();
                scala.Children.Clear();
                scala.ColumnDefinitions.Clear();
                scalaNumbers.Children.Clear();
                scalaNumbers.ColumnDefinitions.Clear();
                clientResult.Children.Clear();
                clientResult.ColumnDefinitions.Clear();

                IList<RatingMeaning> list = repo.findLabelsWithMeanings(min, selectedItem);
                if (list.Count == 0)
                {
                    diagrams.Visibility = Visibility.Hidden;
                    begin.Visibility = Visibility.Hidden;
                    noDiagram.Visibility = Visibility.Visible;

                }
                else
                {
                    noDiagram.Visibility = Visibility.Hidden;
                    begin.Visibility = Visibility.Hidden;
                    diagrams.Visibility = Visibility.Visible;
                    int i = 0;
                    double findClientScoreLineWidth = 0;

                    for (int j = 0; j < list.Count; j++)
                    {
                        RatingMeaning obj = list[j];
                        StackPanel stack = new StackPanel();
                        stack.Orientation = Orientation.Horizontal;
                        stack.Margin = new System.Windows.Thickness(5, 0, 5, 0);
                        stack.VerticalAlignment = VerticalAlignment.Center;
                        TextBlock txtBlock = new TextBlock();

                        txtBlock.Text = " " + obj.name;
                        Rectangle rec = new Rectangle();
                        Rectangle line = new Rectangle();

                        if (obj.rating == 0)
                        {
                            rec.Fill = System.Windows.Media.Brushes.Red;
                            line.Fill = System.Windows.Media.Brushes.Red;
                        }
                        else if (obj.rating == 1)
                        {
                            rec.Fill = System.Windows.Media.Brushes.Orange;
                            line.Fill = System.Windows.Media.Brushes.Orange;
                        }
                        else if (obj.rating == 2)
                        {
                            rec.Fill = System.Windows.Media.Brushes.Yellow;
                            line.Fill = System.Windows.Media.Brushes.Yellow;
                        }
                        else if (obj.rating == 3)
                        {
                            rec.Fill = System.Windows.Media.Brushes.Green;
                            line.Fill = System.Windows.Media.Brushes.Green;
                        }
                        else if (obj.rating == 4)
                        {
                            rec.Fill = System.Windows.Media.Brushes.Blue;
                            line.Fill = System.Windows.Media.Brushes.Blue;
                        }
                        rec.Height = 10;
                        rec.Width = 10;
                        stack.Children.Add(rec);
                        stack.Children.Add(txtBlock);
                        stackpanel.Children.Add(stack);


                        line.Height = 5;
                        if (this.client.gender == "1" && (j + 1) != list.Count)
                        {
                            ColumnDefinition c1 = new ColumnDefinition();
                            c1.Width = new GridLength(Double.Parse((list[j + 1].normM - obj.normM).ToString()), GridUnitType.Star);

                            ColumnDefinition c2 = new ColumnDefinition();
                            c2.Width = new GridLength(Double.Parse((list[j + 1].normM - obj.normM).ToString()), GridUnitType.Star);

                            TextBlock txtBlock2 = new TextBlock();
                            txtBlock2.Text = obj.normM.ToString();
                            txtBlock2.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#778899"));

                            scala.ColumnDefinitions.Add(c1);
                            scala.Children.Add(line);
                            scalaNumbers.ColumnDefinitions.Add(c2);
                            scalaNumbers.Children.Add(txtBlock2);

                            line.Width = 1000;
                            Grid.SetColumn(line, i);
                            Grid.SetColumn(txtBlock2, i);
                            i++;
                            findClientScoreLineWidth += Double.Parse((list[j + 1].normM - obj.normM).ToString());
                        }
                        else if (this.client.gender == "1")
                        {
                            ColumnDefinition c1 = new ColumnDefinition();
                            c1.Width = new GridLength(25, GridUnitType.Star);

                            ColumnDefinition c2 = new ColumnDefinition();
                            c2.Width = new GridLength(25, GridUnitType.Star);

                            TextBlock txtBlock2 = new TextBlock();
                            txtBlock2.Text = obj.normM.ToString();
                            txtBlock2.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#778899"));

                            scala.ColumnDefinitions.Add(c1);
                            scala.Children.Add(line);
                            scalaNumbers.ColumnDefinitions.Add(c2);
                            scalaNumbers.Children.Add(txtBlock2);

                            line.Width = 1000;
                            Grid.SetColumn(line, i);
                            Grid.SetColumn(txtBlock2, i);
                            i++;
                            findClientScoreLineWidth += 25;
                        }
                        else if (this.client.gender == "0" && (j + 1) != list.Count)
                        {
                            ColumnDefinition c1 = new ColumnDefinition();
                            c1.Width = new GridLength(Double.Parse((list[j + 1].normF - obj.normF).ToString()), GridUnitType.Star);

                            ColumnDefinition c2 = new ColumnDefinition();
                            c2.Width = new GridLength(Double.Parse((list[j + 1].normF - obj.normF).ToString()), GridUnitType.Star);

                            TextBlock txtBlock2 = new TextBlock();
                            txtBlock2.Text = obj.normF.ToString();
                            txtBlock2.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#778899"));

                            scala.ColumnDefinitions.Add(c1);
                            scala.Children.Add(line);
                            scalaNumbers.ColumnDefinitions.Add(c2);
                            scalaNumbers.Children.Add(txtBlock2);

                            line.Width = 1000;
                            Grid.SetColumn(line, i);
                            Grid.SetColumn(txtBlock2, i);

                            i++;
                            findClientScoreLineWidth += Double.Parse((list[j + 1].normF - obj.normF).ToString());
                        }
                        else if (this.client.gender == "0")
                        {
                            ColumnDefinition c1 = new ColumnDefinition();
                            c1.Width = new GridLength(25, GridUnitType.Star);

                            ColumnDefinition c2 = new ColumnDefinition();
                            c2.Width = new GridLength(25, GridUnitType.Star);

                            TextBlock txtBlock2 = new TextBlock();
                            txtBlock2.Text = obj.normF.ToString();
                            txtBlock2.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#778899"));

                            scala.ColumnDefinitions.Add(c1);
                            scala.Children.Add(line);
                            scalaNumbers.ColumnDefinitions.Add(c2);
                            scalaNumbers.Children.Add(txtBlock2);

                            line.Width = 1000;
                            Grid.SetColumn(line, i);
                            Grid.SetColumn(txtBlock2, i);
                            i++;
                            findClientScoreLineWidth += 25;
                        }
                    }


                    Rectangle line2 = new Rectangle();
                    line2.Fill = System.Windows.Media.Brushes.Black;

                    Rectangle line3 = new Rectangle();
                    line3.Fill = System.Windows.Media.Brushes.White;

                    line2.Height = 5;
                    

                    line3.Height = 5;
                   

                    ColumnDefinition c3 = new ColumnDefinition();
                    c3.Width = new GridLength(Double.Parse(elem.score.ToString()), GridUnitType.Star);
                    ColumnDefinition c4 = new ColumnDefinition();
                    c4.Width = new GridLength(findClientScoreLineWidth- Double.Parse(elem.score.ToString()), GridUnitType.Star);


                    clientResult.ColumnDefinitions.Add(c3);
                    clientResult.ColumnDefinitions.Add(c4);
                    clientResult.Children.Add(line2);
                    clientResult.Children.Add(line3);
                    
                    Grid.SetColumn(line2, 0);
                    Grid.SetColumn(line3, 1);

                    //Rectangle line2 = new Rectangle();
                    //line2.Fill = System.Windows.Media.Brushes.Yellow;
                    //line2.Height = 5;
                    //line2.Width = 50;
                    //scala.Children.Add(line);
                    //scala.Children.Add(line2);
                }
            }
        }

       

        private class CatsItem
        {
            public TestCategory category { get; set; }
            public Brush color { get; set; }
            public string percentage { get; set; }
        }

        public class ByteArrayComparer : IEqualityComparer<byte[]>
        {
            public bool Equals(byte[] a, byte[] b)
            {
                return a.SequenceEqual(b);
            }

            public int GetHashCode(byte[] key)
            {
                if (key == null)
                    throw new ArgumentNullException("key");
                return key.Sum(b => b);
            }
        }

        private void catsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedIndex != -1)
            {
                TestCategory cat = (TestCategory)grid.SelectedValue;

                var repo = new AppraisalsRepository();
                IList<HistoryTableItem> catHistory = repo.FindByCategory(client, cat);
                dataGrid.ItemsSource = findFullHistory(catHistory);
                diagrams.Visibility = Visibility.Hidden;
                begin.Visibility = Visibility.Visible;
                noDiagram.Visibility = Visibility.Hidden;
                noteMessage.Visibility = Visibility.Visible;
                notesGrid.Visibility = Visibility.Hidden;
            }
        }

        //Creating list with structure: (TestName, units, (date, score, appraiser))
        private List<FullHistoryDatagrid> findFullHistory(IList<HistoryTableItem> items)
        {
            List<FullHistoryDatagrid> result = new List<FullHistoryDatagrid>();

            foreach (HistoryTableItem item in items)
            {
                if (!result.Exists(x => x.TestName == item.TestName))
                {
                    FullHistoryDatagrid newOne = new FullHistoryDatagrid();
                    newOne.TestName = item.TestName;
                    newOne.units = item.Units;
                    newOne.tId = item.tId;
                    newOne.list = new List<Date_Score_Appraiser>();

                    foreach (string date in this.dates)
                    {
                        if (item.date != date)
                        {
                            Date_Score_Appraiser newOne2 = new Date_Score_Appraiser();
                            newOne2.date = date;
                            newOne2.note = "";
                            newOne2.appraiser = item.AppraisersName;
                            newOne2.score = 0;
                            newOne2.applId = null;
                            newOne2.tId = item.tId;
                            newOne.list.Add(newOne2);
                        }
                        else
                        {
                            Date_Score_Appraiser newOne2 = new Date_Score_Appraiser();
                            newOne2.date = item.date;
                            newOne2.appraiser = item.AppraisersName;
                            newOne2.score = item.Score;
                            newOne2.applId = item.applId;
                            newOne2.note = item.note;
                            newOne2.tId = item.tId;
                            newOne.list.Add(newOne2);
                        }
                    }
                    result.Add(newOne);
                }

                else
                {
                    FullHistoryDatagrid getElem = result.Find(x => x.TestName == item.TestName);
                    foreach (Date_Score_Appraiser elem in getElem.list)
                    {
                        if (item.date == elem.date)
                        {
                            elem.appraiser = item.AppraisersName;
                            elem.date = item.date;
                            elem.score = item.Score;
                            elem.applId = item.applId;
                            elem.note = item.note;
                            elem.tId = item.tId;
                        }
                    }
                }
            }
            return result;
        }

        private void allCatsButton_Click(object sender, RoutedEventArgs e)
        {
            catsDataGrid.SelectedIndex = -1;
            var repo = new AppraisalsRepository();
            IList<HistoryTableItem> history = repo.FindAll(client);
            dataGrid.ItemsSource = findFullHistory(history);
            diagrams.Visibility = Visibility.Hidden;
            noDiagram.Visibility = Visibility.Hidden;
            begin.Visibility = Visibility.Visible;
            noteMessage.Visibility = Visibility.Visible;
            notesGrid.Visibility = Visibility.Hidden;
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                btn_Back(null, null);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this);
        }

        private void notesFullText_LostFocus(object sender, RoutedEventArgs e)
        {
            int index = dataGrid.SelectedCells[0].Column.DisplayIndex;
            if (index > -1) {
                Appraisal_tests_repository repo = new Appraisal_tests_repository();
                Appraisal_tests elem = new Appraisal_tests();

                FullHistoryDatagrid selectedItem = (FullHistoryDatagrid)dataGrid.SelectedCells[0].Item;

                
                Date_Score_Appraiser elem2 = selectedItem.list[index - 2];


                elem.testId = elem2.tId;
                elem.appraisalId = elem2.applId;
                elem.note = notesFullText.Text;

                repo.UpdateNote(elem);
            }

          

        }

        private void dataGrid_GotFocus(object sender, RoutedEventArgs e)
        {
           
           


            try
                {
                (e.OriginalSource as DataGridCell).IsSelected = true;
                string columnHeader = (string)dataGrid.SelectedCells[0].Column.Header;

                if(columnHeader == "Test name"|| columnHeader == "Units") {
                    notesGrid.Visibility = Visibility.Hidden;
                    noteMessage.Visibility = Visibility.Visible;
                    diagrams.Visibility = Visibility.Hidden;
                    begin.Visibility = Visibility.Visible;
                    noDiagram.Visibility = Visibility.Hidden;
                }
                // Finding the selected cell object
                FullHistoryDatagrid selectedItem = (FullHistoryDatagrid)dataGrid.SelectedCells[0].Item;

                int index = dataGrid.SelectedCells[0].Column.DisplayIndex;
                Date_Score_Appraiser elem = selectedItem.list[index - 2];

                // Showing Notes Grid
                notesGrid.Visibility = Visibility.Visible;
                noteMessage.Visibility = Visibility.Hidden;

                notesFullText.DataContext = elem;

                // Finding client age range
                RatingRepository repo = new RatingRepository();
                List<Rating> ratingsByTestId = repo.getRatingsByTestId(selectedItem).ToList();

                var ageslist = new SortedSet<int>();

                int min = 0;
                int max = 0;

                foreach (Rating rating in ratingsByTestId)
                {
                    ageslist.Add(rating.age);
                }

                foreach (int setElem in ageslist)
                {
                    if (setElem < int.Parse(client.age))
                    {
                        min = setElem;
                    }
                    else
                    {
                        max = setElem;
                        break;
                    }
                }
                if (max <= 0)
                {
                    ageslabel.Text = "ages " + min.ToString() + "+";
                }
                else
                {
                    ageslabel.Text = "ages " + min.ToString() + "-" + (max - 1).ToString();
                }

                // Finding rating labels with meanings
                stackpanel.Children.Clear();
                scala.Children.Clear();
                scala.ColumnDefinitions.Clear();
                scalaNumbers.Children.Clear();
                scalaNumbers.ColumnDefinitions.Clear();
                clientResult.Children.Clear();
                clientResult.ColumnDefinitions.Clear();

                IList<RatingMeaning> list = repo.findLabelsWithMeanings(min, selectedItem);
                if (list.Count == 0)
                {
                    diagrams.Visibility = Visibility.Hidden;
                    begin.Visibility = Visibility.Hidden;
                    noDiagram.Visibility = Visibility.Visible;
                    noteMessage.Visibility = Visibility.Hidden;
                    notesGrid.Visibility = Visibility.Visible;

                }
                else
                {
                    noDiagram.Visibility = Visibility.Hidden;
                    begin.Visibility = Visibility.Hidden;
                    diagrams.Visibility = Visibility.Visible;
                    noteMessage.Visibility = Visibility.Hidden;
                    notesGrid.Visibility = Visibility.Visible;

                    int i = 0;
                    double findClientScoreLineWidth = 0;

                    for (int j = 0; j < list.Count; j++)
                    {
                        RatingMeaning obj = list[j];
                        StackPanel stack = new StackPanel();
                        stack.Orientation = Orientation.Horizontal;
                        stack.Margin = new System.Windows.Thickness(5, 0, 5, 0);
                        stack.VerticalAlignment = VerticalAlignment.Center;
                        TextBlock txtBlock = new TextBlock();

                        txtBlock.Text = " " + obj.name;
                        Ellipse rec = new Ellipse();
                        Rectangle line = new Rectangle();

                        if (obj.rating == 0)
                        {
                            rec.Fill = System.Windows.Media.Brushes.Red;
                            line.Fill = System.Windows.Media.Brushes.Red;
                        }
                        else if (obj.rating == 1)
                        {
                            rec.Fill = System.Windows.Media.Brushes.Orange;
                            line.Fill = System.Windows.Media.Brushes.Orange;
                        }
                        else if (obj.rating == 2)
                        {
                            rec.Fill = System.Windows.Media.Brushes.Yellow;
                            line.Fill = System.Windows.Media.Brushes.Yellow;
                        }
                        else if (obj.rating == 3)
                        {
                            rec.Fill = System.Windows.Media.Brushes.Green;
                            line.Fill = System.Windows.Media.Brushes.Green;
                        }
                        else if (obj.rating == 4)
                        {
                            rec.Fill = System.Windows.Media.Brushes.Blue;
                            line.Fill = System.Windows.Media.Brushes.Blue;
                        }
                        rec.Height = 10;
                        rec.Width = 10;
                        stack.Children.Add(rec);
                        stack.Children.Add(txtBlock);
                        stackpanel.Children.Add(stack);

                        line.Height = 5;
                        if (this.client.gender == "1" && (j + 1) != list.Count)
                        {
                            ColumnDefinition c1 = new ColumnDefinition();
                            c1.Width = new GridLength(Double.Parse((list[j + 1].normM - obj.normM).ToString()), GridUnitType.Star);

                            ColumnDefinition c2 = new ColumnDefinition();
                            c2.Width = new GridLength(Double.Parse((list[j + 1].normM - obj.normM).ToString()), GridUnitType.Star);

                            TextBlock txtBlock2 = new TextBlock();
                            txtBlock2.Text = obj.normM.ToString();
                            txtBlock2.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#778899"));

                            scala.ColumnDefinitions.Add(c1);
                            scala.Children.Add(line);
                            scalaNumbers.ColumnDefinitions.Add(c2);
                            scalaNumbers.Children.Add(txtBlock2);

                            line.Width = 1000;
                            Grid.SetColumn(line, i);
                            Grid.SetColumn(txtBlock2, i);
                            i++;
                            findClientScoreLineWidth += Double.Parse((list[j + 1].normM - obj.normM).ToString());
                        }
                        else if (this.client.gender == "1")
                        {
                            ColumnDefinition c1 = new ColumnDefinition();
                            c1.Width = new GridLength(25, GridUnitType.Star);

                            ColumnDefinition c2 = new ColumnDefinition();
                            c2.Width = new GridLength(25, GridUnitType.Star);

                            TextBlock txtBlock2 = new TextBlock();
                            txtBlock2.Text = obj.normM.ToString();
                            txtBlock2.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#778899"));

                            scala.ColumnDefinitions.Add(c1);
                            scala.Children.Add(line);
                            scalaNumbers.ColumnDefinitions.Add(c2);
                            scalaNumbers.Children.Add(txtBlock2);

                            line.Width = 1000;
                            Grid.SetColumn(line, i);
                            Grid.SetColumn(txtBlock2, i);
                            i++;
                            findClientScoreLineWidth += 25;
                        }
                        else if (this.client.gender == "0" && (j + 1) != list.Count)
                        {
                            ColumnDefinition c1 = new ColumnDefinition();
                            c1.Width = new GridLength(Double.Parse((list[j + 1].normF - obj.normF).ToString()), GridUnitType.Star);

                            ColumnDefinition c2 = new ColumnDefinition();
                            c2.Width = new GridLength(Double.Parse((list[j + 1].normF - obj.normF).ToString()), GridUnitType.Star);

                            TextBlock txtBlock2 = new TextBlock();
                            txtBlock2.Text = obj.normF.ToString();
                            txtBlock2.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#778899"));

                            scala.ColumnDefinitions.Add(c1);
                            scala.Children.Add(line);
                            scalaNumbers.ColumnDefinitions.Add(c2);
                            scalaNumbers.Children.Add(txtBlock2);

                            line.Width = 1000;
                            Grid.SetColumn(line, i);
                            Grid.SetColumn(txtBlock2, i);

                            i++;
                            findClientScoreLineWidth += Double.Parse((list[j + 1].normF - obj.normF).ToString());
                        }
                        else if (this.client.gender == "0")
                        {
                            ColumnDefinition c1 = new ColumnDefinition();
                            c1.Width = new GridLength(25, GridUnitType.Star);

                            ColumnDefinition c2 = new ColumnDefinition();
                            c2.Width = new GridLength(25, GridUnitType.Star);

                            TextBlock txtBlock2 = new TextBlock();
                            txtBlock2.Text = obj.normF.ToString();
                            txtBlock2.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#778899"));

                            scala.ColumnDefinitions.Add(c1);
                            scala.Children.Add(line);
                            scalaNumbers.ColumnDefinitions.Add(c2);
                            scalaNumbers.Children.Add(txtBlock2);

                            line.Width = 1000;
                            Grid.SetColumn(line, i);
                            Grid.SetColumn(txtBlock2, i);
                            i++;
                            findClientScoreLineWidth += 25;
                        }
                    }


                    Rectangle line2 = new Rectangle();
                    line2.Fill = System.Windows.Media.Brushes.Black;

                    Rectangle line3 = new Rectangle();
                    line3.Fill = System.Windows.Media.Brushes.White;

                    line2.Height = 5;
                    line3.Height = 5;

                    DataGridCell cell = (DataGridCell)e.OriginalSource;
                    TextBlock x = (TextBlock)cell.Content;                    

                    ColumnDefinition c3 = new ColumnDefinition();
                    c3.Width = new GridLength(Double.Parse(x.Text.ToString()), GridUnitType.Star);
                    ColumnDefinition c4 = new ColumnDefinition();
                    c4.Width = new GridLength(findClientScoreLineWidth - Double.Parse(x.Text.ToString()), GridUnitType.Star);


                    clientResult.ColumnDefinitions.Add(c3);
                    clientResult.ColumnDefinitions.Add(c4);
                    clientResult.Children.Add(line2);
                    clientResult.Children.Add(line3);

                    Grid.SetColumn(line2, 0);
                    Grid.SetColumn(line3, 1);

                    //Rectangle line2 = new Rectangle();
                    //line2.Fill = System.Windows.Media.Brushes.Yellow;
                    //line2.Height = 5;
                    //line2.Width = 50;
                    //scala.Children.Add(line);
                    //scala.Children.Add(line2);
                }
            }
            catch { }
        }

        
    }
}
