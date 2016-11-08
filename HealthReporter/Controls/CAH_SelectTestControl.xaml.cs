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
    public partial class CAH_SelectTestControl : UserControl
    {
        private Client client;
        private MainWindow _parent;
        private Group group;
        private Appraiser appraiser;
        private Appraisal appraisal;
        private List<Test> tests = new List<Test>();
     

        public CAH_SelectTestControl(MainWindow _parent, Client client, Group group, Appraiser appraiser, Appraisal appraisal)
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
                // MessageBox.Show(test.name, "Message");
            }
            if (tests.Count < 1)
            {
                MessageBox.Show("Please select test/tests.", "Message");
            }
            else
            {
                int childNumber = this._parent.stkTest.Children.Count;
                this._parent.stkTest.Children.RemoveRange(childNumber - 3, childNumber);
                CAH obj = new CAH(this._parent, client, group);
                this._parent.stkTest.Children.Add(obj);


                //try
                //{


                //    List<Appraisal_tests> testsList = new List<Appraisal_tests>();

                //    for (int i = 0; i < listBox.Items.Count; i++)
                //    {
                //        ContentPresenter c = (ContentPresenter)listBox.ItemContainerGenerator.ContainerFromItem(listBox.Items[i]);

                //        Test test = tests[i];



                //        //Appraisal_tests object
                //        Appraisal_tests o = new Appraisal_tests();
                //        o.appraisalId = appraisal.id;
                //        o.testId = test.id;
                //        o.score = decimal.Parse("2.5");
                //        o.note = "note";
                //        o.trial1 = decimal.Parse("2.5");
                //        o.trial2 = 0;
                //        o.trial3 = 0;

                //        testsList.Add(o);
                //    }




                //    AppraisalsRepository repo = new AppraisalsRepository();
                //    repo.Insert(appraisal, appraiser, testsList);


                //    this._parent.stkTest.Children.Clear();
                //    CAH obj = new CAH(this._parent, client, group);
                //    this._parent.stkTest.Children.Add(obj);
                //}
                //catch
                //{
                //    MessageBox.Show("You must insert at least one Trial for every test or you entered something wrong into the trials fields", "Message");
                //}
            }
        }
    }
}
