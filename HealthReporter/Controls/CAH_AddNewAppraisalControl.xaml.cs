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
    /// Interaction logic for CAH_AddNewAppraisal.xaml
    /// </summary>
    public partial class CAH_AddNewAppraisalControl : UserControl
    {
        private Client client;
        private Group group;
        private List<Test> tests;
        private MainWindow _parent;

        public CAH_AddNewAppraisalControl()
        {
            InitializeComponent();
        }

        public CAH_AddNewAppraisalControl(MainWindow _parent, Client client, List<Test> tests, Group group)
        {
            InitializeComponent();
            this._parent = _parent;
            this.client = client;
            this.tests = tests;
            this.group = group;

            AddFields.ItemsSource = tests;
        }

        private void btn_Back(object sender, RoutedEventArgs e)
        {
            this._parent.stkTest.Children.Clear();
            //CAH_SelectTestControl obj = new CAH_SelectTestControl(this._parent, client, group, appraiser, appraisal);
            //this._parent.stkTest.Children.Add(obj);
        }

        private void btn_OK(object sender, RoutedEventArgs e)
        {
            //var res = connection.InsertSql("INSERT INTO appraisals (id, appraiserId, clientId, date) values(@id, @appraiserId, @clientId, @date)", appraisal);
            //var res2 = connection.InsertSql("INSERT INTO appraisers (id, name) values(@id, @name)", appraiser);
            //var res3 = connection.InsertSql("INSERT INTO appraisal_tests (appraisalId, testId, score, note, trial1, trial2, trial3) values(@appraisalId, @testId, @score, @note, @trial1, @trial2, @trial3)", test);

            if (String.IsNullOrWhiteSpace(this.date.Text) || String.IsNullOrWhiteSpace(this.name.Text))
            {
                MessageBox.Show("You forgot to enter Date or Appraisers Name", "Message");

            }
            else
            {


                try
                {

                    //Appraiser object
                    Appraiser appraiser = new Appraiser();
                    appraiser.id = System.Guid.NewGuid().ToByteArray();
                    appraiser.name = name.Text.ToString();

                    //Appraisal object
                    Appraisal appraisal = new Appraisal();
                    appraisal.id = System.Guid.NewGuid().ToByteArray();
                    appraisal.appraiserId = appraiser.id;
                    appraisal.clientId = client.id;
                    DateTime enteredDate = Convert.ToDateTime(date.SelectedDate.ToString());
                    appraisal.date = String.Format("{0:yyyy-MM-dd}", enteredDate);



                    List<Appraisal_tests> testsList = new List<Appraisal_tests>();

                    for (int i = 0; i < AddFields.Items.Count; i++)
                    {
                        ContentPresenter c = (ContentPresenter)AddFields.ItemContainerGenerator.ContainerFromItem(AddFields.Items[i]);

                        Test test = tests[i];

                        TextBox tb = c.ContentTemplate.FindName("Trial1", c) as TextBox;
                        TextBox tb2 = c.ContentTemplate.FindName("Trial2", c) as TextBox;
                        TextBox tb3 = c.ContentTemplate.FindName("Trial3", c) as TextBox;




                        //Appraisal_tests object
                        Appraisal_tests o = new Appraisal_tests();
                        o.appraisalId = appraisal.id;
                        o.testId = test.id;
                        o.score = decimal.Parse(tb.Text);
                        o.note = "note";
                        o.trial1 = decimal.Parse(tb.Text);
                        o.trial2 = 0;
                        o.trial3 = 0;

                        testsList.Add(o);
                    }




                    AppraisalsRepository repo = new AppraisalsRepository();
                    repo.Insert(appraisal, appraiser, testsList);


                    this._parent.stkTest.Children.Clear();
                    CAH obj = new CAH(this._parent, client, group);
                    this._parent.stkTest.Children.Add(obj);
                }
                catch
                {
                    MessageBox.Show("You must insert at least one Trial for every test or you entered something wrong into the trials fields", "Message");
                }
            }
        }
        }
    }

