using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HealthReporter;
using TestStack.White.UIItems.MenuItems;
using TestStack.White.UIItems;
using TestStack.White;
using System.IO;
using TestStack.White.UIItems.WindowItems;
using TestStack.White.Factory;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.WPFUIItems;
using TestStack.White.UIItems.ListBoxItems;

// https://msdn.microsoft.com/en-us/library/ms182532.aspx

namespace HealthReporterUnitTests
{
    [TestClass]
    public class MainWindowViewModelTests
    {
        private TestContext _testContext;

        public TestContext TestContext
        {
            get { return _testContext; }
            set { _testContext = value; }
        }

        //[TestMethod]
        //public void TestMethod1()
        //{
        //    // arrange
        //    double a = 11.99;
        //    double b = 12;
        //    double expectedSum = a+b;

        //    // act
        //    double actualSum = a + b;

        //    // assert
        //    Assert.AreEqual(expectedSum, actualSum, 0.001, "Wrong sum");
        //}

        [TestMethod]
        public void TestAddClientGroup()
        {
            string groupName = "Test grupp";
            string solutionDir = Path.GetDirectoryName(Path.GetDirectoryName(TestContext.TestRunDirectory));
            string applicationPath = Path.Combine(solutionDir, "HealthReporter/bin/Debug/HealthReporter.exe");
            Application application = Application.Launch(applicationPath);
            Window window = application.GetWindow("Health Reporter", InitializeOption.NoCache);

            ListView groups = window.Get<ListView>("groupDataGrid");
            var rows = groups.Rows;
            int groupCount = rows.Count;

            Button button = window.Get<Button>("addStuff");
            button.Click();

            PopUpMenu menu = window.Popup;
            menu.Item("New group").Click();


            groups.SelectedRows[0].Enter(groupName);
            window.Click();

            rows = groups.Rows;
            int newCount = rows.Count;
            string newName = rows[newCount - 1].Name;
            var test = rows[newCount - 1].GetElement(SearchCriteria.All);
            application.Close();
            Assert.AreEqual(groupCount + 1, newCount, 0, "Wrong number of groups after adding one.");
        }

        [TestMethod]
        public void TestAddClient()
        {
            string solutionDir = Path.GetDirectoryName(Path.GetDirectoryName(TestContext.TestRunDirectory));
            string applicationPath = Path.Combine(solutionDir, "HealthReporter/bin/Debug/HealthReporter.exe");
            Application application = Application.Launch(applicationPath);
            Window window = application.GetWindow("Health Reporter", InitializeOption.NoCache);

            ListView groups = window.Get<ListView>("groupDataGrid");
            Button addButton = window.Get<Button>("addStuff");
            PopUpMenu menu;
            if (groups.Rows.Count <= 0)
            {
                addButton.Click();

                menu = window.Popup;
                menu.Item("New group").Click();


                groups.SelectedRows[0].Enter("Test grupp 2");
                window.Click();
            }

            groups.Rows[0].Click();

            ListView clients = window.Get<ListView>("clientDataGrid");
            int clientCount = clients.Rows.Count;

            addButton.Click();
            menu = window.Popup;

            menu.Item("New client").Click();

            TextBox firstName = window.Get<TextBox>("firstName");
            firstName.Enter("Test eesnimi");
            TextBox lastName = window.Get<TextBox>("lastName");
            lastName.Enter("Test perenimi");
            TextBox email = window.Get<TextBox>("email");
            email.Enter("test@test.ee");
            window.Get<Button>("allClientsButton").Click();
            groups.Rows[0].Click();

            int newClientCount = clients.Rows.Count;
            application.Close();
            Assert.AreEqual(clientCount + 1, newClientCount, 0, "Wrong number of clients after adding one.");
        }

        [TestMethod]
        public void TestAddTestCategory()
        {
            string solutionDir = Path.GetDirectoryName(Path.GetDirectoryName(TestContext.TestRunDirectory));
            string applicationPath = Path.Combine(solutionDir, "HealthReporter/bin/Debug/HealthReporter.exe");
            Application application = Application.Launch(applicationPath);
            Window window = application.GetWindow("Health Reporter", InitializeOption.NoCache);

            window.Get<Button>("btnShowTests").Click();
            ListView categories = window.Get<ListView>("catsDataGrid");
            var rows = categories.Rows;
            int catsCount = rows.Count;
            Console.WriteLine(catsCount);

            window.Get<Button>("addStuffButton").Click();

            PopUpMenu menu = window.Popup;
            menu.Item("New category").Click();

            window.Get<TextBox>("name").Enter("Testkategooria");
            window.Get<ComboBox>("parentSelector").Select("");
            window.Get<Button>("createCategory").Click();

            int newCatsCount = window.Get<ListView>("catsDataGrid").Rows.Count;
            Console.WriteLine(newCatsCount);
            application.Close();
            Assert.AreEqual(catsCount + 1, newCatsCount, 0, "Wrong number of test categories after adding one.");
        }
    }
}
