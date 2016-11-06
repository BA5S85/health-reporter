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
        private List<Test> tests = new List<Test>();
     

        public CAH_SelectTestControl(MainWindow _parent, Client client)
        {

            var repo = new TestRepository();
            IList<Test> tests = repo.FindAll();
            
            InitializeComponent();
            this._parent = _parent;
            this.client = client;

            
            listBox.ItemsSource = tests;
           
           
        }

        private void btn_Back(object sender, RoutedEventArgs e)
        {
            int childNumber = this._parent.stkTest.Children.Count;
            this._parent.stkTest.Children.RemoveAt(childNumber - 1);

            CAH obj = new CAH(this._parent,client);
            this._parent.stkTest.Children.Add(obj);
        }

        private void btn_OK(object sender, RoutedEventArgs e)
        {
           
            //foreach (var item in listBox.SelectedItems)
            //{
            //    Test test = (Test)item;
            //    tests.Add(test);
            //   // MessageBox.Show(test.name, "Message");
            //}
            //if (tests.Count < 1)
            //{
            //    MessageBox.Show("Please select test/tests.", "Message");
            //}
            //else { 
            //    this._parent.stkTest.Children.Clear();
            //    CAH_AddNewAppraisalControl obj = new CAH_AddNewAppraisalControl(this._parent, client, tests);
            //    this._parent.stkTest.Children.Add(obj);
            //}
        }
    }
}
