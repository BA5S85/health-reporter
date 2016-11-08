using HealthReporter.Models;
using HealthReporter.Utilities;
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

namespace HealthReporter.Controls
{
    /// <summary>
    /// Interaction logic for AddNewTestCategoryControl.xaml
    /// </summary>
    public partial class AddNewTestCategoryControl : UserControl
    {
        public MainWindow _parent;

        public AddNewTestCategoryControl(MainWindow parentWindow, byte[] parentCategory)
        {
            InitializeComponent();
            this._parent = parentWindow;

            grid.DataContext = new TestCategory();

            var repo = new TestCategoryRepository();
        }

        private void btn_Back(object sender, RoutedEventArgs e)
        {
            this._parent.stkTest.Children.Clear();
            TestsUserControl obj = new TestsUserControl(this._parent);
            this._parent.stkTest.Children.Add(obj);
        }

        private void btn_CreateNewTestCategory(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(name.Text))
            {
                return;
            }

            byte[] parentId = null;
            //var parentCategory = (TestCategory)parentSelector.SelectedItem;
            //if (parentCategory != null)
            //{
            //    parentId = parentCategory.id;
            //}
            var testCategory = new TestCategory() { name = this.name.Text, parentId = parentId };

            var repo = new TestCategoryRepository();
            repo.Insert(testCategory);

            this._parent.stkTest.Children.Clear();
            TestsUserControl obj = new TestsUserControl(this._parent);
            this._parent.stkTest.Children.Add(obj);

        }
    }
}
