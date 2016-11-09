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
using System.Collections.Generic;
using TestStack.White.UIItems.TabItems;

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

            window.Get<Button>("addStuffButton").Click();

            PopUpMenu menu = window.Popup;
            menu.Item("New category").Click();

            window.Get<TextBox>("name").Enter("Testkategooria");
            window.Get<Button>("createCategory").Click();

            int newCatsCount = window.Get<ListView>("catsDataGrid").Rows.Count;
            application.Close();
            Assert.AreEqual(catsCount + 1, newCatsCount, 0, "Wrong number of test categories after adding one.");
        }

        [TestMethod]
        public void TestAddTest()
        {
            string solutionDir = Path.GetDirectoryName(Path.GetDirectoryName(TestContext.TestRunDirectory));
            string applicationPath = Path.Combine(solutionDir, "HealthReporter/bin/Debug/HealthReporter.exe");
            Application application = Application.Launch(applicationPath);
            Window window = application.GetWindow("Health Reporter", InitializeOption.NoCache);

            window.Get<Button>("btnShowTests").Click();
            ListView categories = window.Get<ListView>("catsDataGrid");
            Button addButton = window.Get<Button>("addStuffButton");
            PopUpMenu menu;
            if (categories.Rows.Count <= 0)
            {
                addButton.Click();

                menu = window.Popup;
                menu.Item("New category").Click();

                window.Get<TextBox>("name").Enter("Testkategooria");
                window.Get<Button>("createCategory").Click();
            }
            categories = window.Get<ListView>("catsDataGrid");
            categories.Rows[0].Click();

            ListView tests = window.Get<ListView>("testsDataGrid");
            int testsCount = tests.Rows.Count;

            addButton.Click();
            menu = window.Popup;
            menu.Item("New test").Click();

            window.Get<TextBox>("testName").Enter("Test test");
            window.Get<TextBox>("units").Enter("1");

            addButton.Click();
            menu = window.Popup;
            menu.Item("New age group").Click();

            Window modal = window.ModalWindow("Input");
            modal.Get<TextBox>().Enter("0-10");
            modal.Get<Button>("btnDialogOk").Click();

            ListViewCell cell;
            Tab tab = window.Get<Tab>("GenderTab");

            tab.SelectTabPage(0);
            Tab men = tab.Get<Tab>("MenAgesTab");
            men.SelectTabPage(0);
            ListView menRatings = men.Get<ListView>("menRatingsDatagrid");
            cell = menRatings.Cell("Score", 0);
            cell.Click();
            cell.Enter("10");
            cell = menRatings.Cell("Description", 0);
            cell.Click();
            cell.Enter("Bad");
            cell = menRatings.Cell("Score", 1);
            cell.Click();
            cell.Enter("25");
            cell = menRatings.Cell("Description", 1);
            cell.Click();
            cell.Enter("Average");
            men.SelectTabPage(1);
            cell = menRatings.Cell("Score", 0);
            cell.Click();
            cell.Enter("20");
            cell = menRatings.Cell("Description", 0);
            cell.Click();
            cell.Enter("Bad");
            cell = menRatings.Cell("Score", 1);
            cell.Click();
            cell.Enter("50");
            cell = menRatings.Cell("Description", 1);
            cell.Click();
            cell.Enter("Average");

            tab.SelectTabPage(1);
            Tab women = tab.Get<Tab>("WomenAgesTab");
            women.SelectTabPage(1);
            ListView womenRatings = women.Get<ListView>("WomenRatingsDatagrid");
            cell = womenRatings.Cell("Score", 0);
            cell.Click();
            cell.Enter("5");
            cell = womenRatings.Cell("Description", 0);
            cell.Click();
            cell.Enter("Bad");
            cell = womenRatings.Cell("Score", 1);
            cell.Click();
            cell.Enter("10");
            cell = womenRatings.Cell("Description", 1);
            cell.Click();
            cell.Enter("Average");
            women.SelectTabPage(1);
            cell = womenRatings.Cell("Score", 0);
            cell.Click();
            cell.Enter("15");
            cell = womenRatings.Cell("Description", 0);
            cell.Click();
            cell.Enter("Bad");
            cell = womenRatings.Cell("Score", 1);
            cell.Click();
            cell.Enter("30");
            cell = womenRatings.Cell("Description", 1);
            cell.Click();
            cell.Enter("Bad");
            womenRatings.Select(1);

            window.Get<TextBox>("search").Click();
            categories.Rows[0].Click();

            int newTestsCount = window.Get<ListView>("testsDataGrid").Rows.Count;
            application.Close();
            Assert.AreEqual(testsCount + 1, newTestsCount, 0, "Wrong number of tests after adding one.");
        }
    }
}
