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

        [TestMethod]
        public void TestMethod1()
        {
            // arrange
            double a = 11.99;
            double b = 12;
            double expectedSum = a+b;

            // act
            double actualSum = a + b;

            // assert
            Assert.AreEqual(expectedSum, actualSum, 0.001, "Wrong sum");
        }

        [TestMethod]
        public void TestAppWhite()
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
    }
}
