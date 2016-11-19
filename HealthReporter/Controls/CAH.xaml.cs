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

            ClientInfo.DataContext = client1;
            ClientGroup.DataContext = group;

            //Finding all appraisal dates of client
            List<string> dates = new List<string>();

            //Finding all appraisal results of client
            var repo = new AppraisalsRepository();

            IList<HistoryTableItem> history = repo.FindAll(client);
            foreach (HistoryTableItem item in history)
            {
                if (item.date != null && !dates.Contains(item.date.ToString()))
                {
                    dates.Add(item.date.ToString());
                }
            }

            dates.Sort((x, y) => DateTime.Parse(y).CompareTo(DateTime.Parse(x)));

            //Creating list with structure: (TestName, units, (date, score, appraiser))

            //Initializing datagrid objects
            List<FullHistoryDatagrid> result = new List<FullHistoryDatagrid>();

            foreach (HistoryTableItem item in history)
            {
                if (!result.Exists(x => x.TestName == item.TestName))
                {
                    FullHistoryDatagrid newOne = new FullHistoryDatagrid();
                    newOne.TestName = item.TestName;
                    newOne.units = item.Units;
                    newOne.tId = item.tId;
                    newOne.list = new List<Date_Score_Appraiser>();

                    foreach (string date in dates)
                    {
                        if (item.date != date)
                        {
                            Date_Score_Appraiser newOne2 = new Date_Score_Appraiser();
                            newOne2.date = date;
                            newOne2.appraiser = item.AppraisersName;
                            newOne2.score = 0;
                            newOne2.applId = null;
                            newOne2.tId= item.tId;
                            newOne.list.Add(newOne2);
                        }
                        else
                        {
                            Date_Score_Appraiser newOne2 = new Date_Score_Appraiser();
                            newOne2.date = item.date;
                            newOne2.appraiser = item.AppraisersName;
                            newOne2.score = item.Score;
                            newOne2.applId = item.applId;
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
                                    elem.tId = item.tId;
                                }                                                        
                    }
                }
            }

            //Reading elements into table
            int i = 0;
            foreach (string elem in dates)
            {
                DataGridTextColumn textColumn = new DataGridTextColumn();
                textColumn.Header = String.Format("{0:dd/MM/yyyy}", DateTime.Parse(elem));
                Binding binding =   new Binding("list[" + i + "]");
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
            FullHistoryDatagrid elem = (FullHistoryDatagrid)dataGrid.SelectedItem;

            DataGridColumn col1 = e.Column;
            int index = col1.DisplayIndex;

            Date_Score_Appraiser elem2 = elem.list[index - 2];
            var repoAT= new Appraisal_tests_repository();

            IList<Appraisal_tests> history = repoAT.FindAll();

            Appraisal_tests appTest = new Appraisal_tests();
            appTest.testId = elem2.tId;
            appTest.appraisalId = elem2.applId;
            appTest.score = elem2.score;

            var repo = new Appraisal_tests_repository();
           
            if (editedTextbox.Text.ToString() != "")
                {
                if (elem2.applId!=null)
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
                }else {                    
                    foreach(FullHistoryDatagrid item in dataGrid.Items)
                    {
                        foreach(Date_Score_Appraiser item2 in item.list)
                        {
                            if (item2.date == elem2.date && item2.applId!=null)
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
        }

        private int rowIndex = 0;
        private int columnIndex=0;
            

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

                foreach(Rating rating in ratingsByTestId)
                {
                    ageslist.Add(rating.age);
                }

                foreach (int setElem in ageslist)
                {
                    
                    if ( setElem < int.Parse(client.age))
                    {
                        min = setElem;

                    }else
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

                IList<RatingMeaning> list = repo.findLabelsWithMeanings(min,selectedItem);
                if (list.Count == 0)
                {
                    diagrams.Visibility = Visibility.Hidden;
                    noDiagram.Visibility = Visibility.Visible;
                  
                }else {
                    noDiagram.Visibility = Visibility.Hidden;
                    diagrams.Visibility = Visibility.Visible;
                    int i = 0;

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
                    else if(obj.rating == 1) {
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
                    else if  (obj.rating == 4)
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
                    if (this.client.gender == "1" && (j+1)!=list.Count)
                    {
                        ColumnDefinition c1 = new ColumnDefinition();
                        c1.Width = new GridLength(Double.Parse((list[j+1].normM-obj.normM).ToString()), GridUnitType.Star);

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
                    }else if (this.client.gender == "1")
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
                    }

                 
                }

             
                //Rectangle line2 = new Rectangle();
                //line2.Fill = System.Windows.Media.Brushes.Black;

                //line2.Height = 5;
                //line2.Width = 50;

                //ColumnDefinition c3 = new ColumnDefinition();
                //c3.Width = new GridLength(Double.Parse(elem.score.ToString()), GridUnitType.Star);

              
                //clientResult.ColumnDefinitions.Add(c3);
                //clientResult.Children.Add(line2);
                //line2.Width = 1000;
                //Grid.SetColumn(line2, 0);
               
                //Rectangle line2 = new Rectangle();
                //line2.Fill = System.Windows.Media.Brushes.Yellow;
                //line2.Height = 5;
                //line2.Width = 50;
                //scala.Children.Add(line);
                //scala.Children.Add(line2);
            }
        }
        }

        DataGridColumnHeader lastObject;

        private void getHeaderName(object sender, RoutedEventArgs e)
        {
            
            if (lastObject != null)
            {
                lastObject.Background = Brushes.White;
                lastObject.BorderBrush = Brushes.LightGray;
            }
           
            DataGridColumnHeader header = ((DataGridColumnHeader)sender);
            string headerText = header.Content.ToString();

            if(headerText != "TestName" && headerText != "units") {
            lastObject = header;
            header.Background= Brushes.Gainsboro;
            
            this.columnIndex = dataGrid.Columns.Single(c => c.Header.ToString() == headerText).DisplayIndex-2;
            dataGrid.SelectedIndex = rowIndex;
            }
        }
    }
}
