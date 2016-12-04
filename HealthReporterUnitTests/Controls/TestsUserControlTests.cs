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
            ListView categories = window.Get<ListView>("catsDataGrid");
            Button addButton = window.Get<Button>("addStuffButton");
            PopUpMenu menu;
            if (categories.Rows.Count <= 0)
            {
                addButton.Click();

                menu = window.Popup;
                menu.Item("New category").Click();

                categories.SelectedRows[0].Enter("Testcategory");
                window.Keyboard.PressSpecialKey(KeyboardInput.SpecialKeys.RETURN);
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

            Button ratingButton = window.Get<Button>("addRatingsM");
            ratingButton.Click();

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
