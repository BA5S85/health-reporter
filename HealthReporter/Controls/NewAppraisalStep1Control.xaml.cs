using HealthReporter.Models;
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
        private Client client;
        private Group group;
        private Appraiser appraiser;
        private Appraisal appraisal;

        public NewAppraisalStep1Control(MainWindow parentWindow, Client client, Group group)
        {
            InitializeComponent();
            this._parent = parentWindow;
            this.client = client;
            this.group = group;
            appraiser = new Appraiser();
            appraiser.id = System.Guid.NewGuid().ToByteArray();
            appraiser.name = "";
            name.DataContext = appraiser;

            appraisal = new Appraisal();
            appraisal.id = System.Guid.NewGuid().ToByteArray();
            appraisal.appraiserId = appraiser.id;
            appraisal.clientId = client.id;
            appraisal.date = String.Format("{0:yyyy-MM-dd}", DateTime.Now);
            date.DataContext = appraisal;

            AdornerSite.Visibility = Visibility.Hidden;
        }

        private void btn_Back(object sender, RoutedEventArgs e)
        {
            int childNumber = this._parent.stkTest.Children.Count;
            this._parent.stkTest.Children.RemoveAt(childNumber - 1);
            this._parent.stkTest.Children[childNumber - 2].Focus();
        }

        private void btn_Next(object sender, RoutedEventArgs e)
        {
            try
            {
                if (appraiser.name == "")
                {
                    AdornerSite.Visibility = Visibility.Visible;
                    throw new Exception();
                }
                DateTime enteredDate = Convert.ToDateTime(date.SelectedDate.ToString());
                appraisal.date = String.Format("{0:yyyy-MM-dd}", enteredDate);

                var repo = new AppraisalsRepository();

                //All appraisal dates
                List<DateTime> dateList = repo.FindAllDates(client).ToList();
                dateList.Sort((x, y) => y.CompareTo(x));                

                //Get tests that are in CAH already and add the ones that are missing
                List<byte[]> tests = new List<byte[]>();
                IList<HistoryTableItem> history = repo.FindAll(client);

                foreach(HistoryTableItem elem in history)
                {    
                    tests.Add(elem.tId);
                }

                if (tests.Count != 0)
                {
                    
                        List<Appraisal_tests> at = new List<Appraisal_tests>();
                        foreach (byte[] test in tests)
                        {
                            if(!dateList.Contains(DateTime.Parse(appraisal.date)))
                            {
                                Appraisal_tests one = new Appraisal_tests();
                                one.appraisalId = appraisal.id;
                                one.testId = test;
                                one.score = Decimal.Parse("0");
                                at.Add(one);
                            }
                            
                        }
                        repo.Insert(at);
                 }

               
                repo.InsertAppraisalAndAppraiser(appraisal, appraiser);

                // Going to the Main appraisal history view
                int childNumber = this._parent.stkTest.Children.Count;
                this._parent.stkTest.Children.RemoveRange(childNumber - 2, childNumber);

                CAH obj = new CAH(this._parent, client, group);

                this._parent.stkTest.Children.Add(obj);
            }
            catch {}
        }

        private void name_GotFocus(object sender, RoutedEventArgs e)
        {
            AdornerSite.Visibility = Visibility.Visible;
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                btn_Back(null, null);
            }
            else if(e.Key == Key.Enter)
            {
                btn_Next(null, null);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this);
        }
    }
}
