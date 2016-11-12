﻿using System;
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

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FullHistoryDatagrid selectedItem = (FullHistoryDatagrid)dataGrid.SelectedItem;

            scala.Children.Clear();

            Rectangle line = new Rectangle();
            line.Fill = System.Windows.Media.Brushes.Green;
            line.Height = 5;
            line.Width = 50;
            Rectangle line2 = new Rectangle();
            line2.Fill = System.Windows.Media.Brushes.Yellow;
            line2.Height = 5;
            line2.Width = 50;
            scala.Children.Add(line);
            scala.Children.Add(line2);

        }
    }
}
