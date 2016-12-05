using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestStack.White.UIItems.MenuItems;
using TestStack.White.UIItems;
using TestStack.White;
using System.IO;
using TestStack.White.UIItems.WindowItems;
using TestStack.White.Factory;
using TestStack.White.UIItems.WPFUIItems;
using TestStack.White.UIItems.TabItems;
using TestStack.White.WindowsAPI;
using HealthReporter.Utilities;
using HealthReporterUnitTests.Utilities;

namespace HealthReporterUnitTests.Controls
{
    [TestClass]
    public class TestsUserControlTests
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

            categories.SelectedRows[0].Enter("Testcategory");
            window.Keyboard.PressSpecialKey(KeyboardInput.SpecialKeys.RETURN);

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

            Helpers.addTestGroupIfNoneExist(window, "Testcategory");

            ListView categories = window.Get<ListView>("catsDataGrid");
            categories.Rows[0].Click();

            ListView tests = window.Get<ListView>("testsDataGrid");
            int testsCount = tests.Rows.Count;

            Helpers.addTest(window, "Test test");

            window.Get<TextBox>("search").Click();
            categories = window.Get<ListView>("catsDataGrid");
            categories.Rows[0].Click();

            int newTestsCount = window.Get<ListView>("testsDataGrid").Rows.Count;
            application.Close();
            Assert.AreEqual(testsCount + 1, newTestsCount, 0, "Wrong number of tests after adding one.");
        }
    }
}
