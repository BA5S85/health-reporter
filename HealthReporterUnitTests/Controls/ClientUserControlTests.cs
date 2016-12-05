using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestStack.White.UIItems.MenuItems;
using TestStack.White.UIItems;
using TestStack.White;
using System.IO;
using TestStack.White.UIItems.WindowItems;
using TestStack.White.Factory;
using TestStack.White.UIItems.Finders;
using TestStack.White.WindowsAPI;
using HealthReporter.Utilities;
using TestStack.White.UIItems.TabItems;
using TestStack.White.UIItems.WPFUIItems;
using HealthReporterUnitTests.Utilities;
using System;
using TestStack.White.UIItems.ListBoxItems;
using TestStack.White.UIA;

namespace HealthReporterUnitTests.Controls
{
    [TestClass]
    public class ClientUserControlTests
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

            [TestInitialize]
            public void DropDatabase()
            {
                DatabaseUtility.resetDb();
            }

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
                window.Keyboard.PressSpecialKey(KeyboardInput.SpecialKeys.RETURN);

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
                
                Helpers.addClientGroupIfNoneExist(window, "Test client category");

                ListView groups = window.Get<ListView>("groupDataGrid");
                groups.Rows[0].Click();

                ListView clients = window.Get<ListView>("clientDataGrid");
                int clientCount = clients.Rows.Count;

                Helpers.addClient(window, "Test client");

                clients = window.Get<ListView>("clientDataGrid");
                int newClientCount = clients.Rows.Count;
                application.Close();
                Assert.AreEqual(clientCount + 1, newClientCount, 0, "Wrong number of clients after adding one.");
            }

            [TestMethod]
            public void TestAddAppraisal()
            {
                string solutionDir = Path.GetDirectoryName(Path.GetDirectoryName(TestContext.TestRunDirectory));
                string applicationPath = Path.Combine(solutionDir, "HealthReporter/bin/Debug/HealthReporter.exe");
                Application application = Application.Launch(applicationPath);
                Window window = application.GetWindow("Health Reporter", InitializeOption.NoCache);

                Helpers.addTestGroupIfNoneExist(window, "Testcategory");

                Helpers.addTest(window, "test 1");
                Helpers.addTest(window, "test 2");
                Helpers.addTest(window, "test 3");

                Helpers.addClientGroupIfNoneExist(window, "Test client category");

                Helpers.addClient(window, "Test client");

                ListView clients = window.Get<ListView>("clientDataGrid");
                clients.Rows[0].Click();

                window.Get<Button>("openAppraisalHistoryBtn").Click();
                window.Get<Button>("button2").Click();

                window.Get<TextBox>("name").Enter("Test Appraiser");
                window.Get<Button>("newAppraisalNext").Click();

                window.Get<Button>("button2_Copy1").Click();

                ListView testCats = window.Get<ListView>("catsDataGrid");
                System.Windows.Point point;

                for (int i = 0; i < 4; i++)
                {
                    point = new System.Windows.Point(testCats.Rows[0].Bounds.TopLeft.X + 40, testCats.Rows[0].Bounds.TopLeft.Y - 70 + i * 30);
                    window.Mouse.Location = point;
                    window.Mouse.Click();
                }

                window.Get<Button>("btnOK").Click();

                ListView results = window.Get<ListView>("dataGrid");
                ListViewCell cell;
                cell = results.Rows[0].Cells[2];
                cell.Click();
                cell.Enter("90");
                window.Keyboard.PressSpecialKey(KeyboardInput.SpecialKeys.RETURN);

                results = window.Get<ListView>("dataGrid");
                cell = results.Rows[1].Cells[2];
                cell.Click();
                cell.Enter("90");
                window.Keyboard.PressSpecialKey(KeyboardInput.SpecialKeys.RETURN);

                results = window.Get<ListView>("dataGrid");
                cell = results.Rows[2].Cells[2];
                cell.Click();
                cell.Enter("90");
                window.Keyboard.PressSpecialKey(KeyboardInput.SpecialKeys.RETURN);

                application.Close();

            }
        }
    }
}
