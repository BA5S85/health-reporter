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
            this.client = client;
            this.group = group;
            this.appraiser = appraiser;
            this.appraisal = appraisal;

            listBox.ItemsSource = tests;           
        }

        private void btn_Back(object sender, RoutedEventArgs e)
        {
            int childNumber = this._parent.stkTest.Children.Count;
            this._parent.stkTest.Children.RemoveAt(childNumber - 1);            
        }

        private void btn_OK(object sender, RoutedEventArgs e)
        {
            foreach (var item in listBox.SelectedItems)
            {
                Test test = (Test)item;
                tests.Add(test);
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
    }
}
