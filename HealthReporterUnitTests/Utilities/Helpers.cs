using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestStack.White.UIItems;
using TestStack.White.UIItems.MenuItems;
using TestStack.White.UIItems.TabItems;
using TestStack.White.UIItems.WindowItems;
using TestStack.White.UIItems.WPFUIItems;
using TestStack.White.WindowsAPI;

namespace HealthReporterUnitTests.Utilities
{
    public class Helpers
    {
        static public void addTestGroupIfNoneExist(Window window, string name)
        {
            window.Get<Button>("btnShowTests").Click();
            ListView categories = window.Get<ListView>("catsDataGrid");
            Button addButton = window.Get<Button>("addStuffButton");
            PopUpMenu menu;
            if (categories.Rows.Count <= 0)
            {
                addButton.Click();

                menu = window.Popup;
                menu.Item("New category").Click();

                categories.SelectedRows[0].Enter(name);
                window.Keyboard.PressSpecialKey(KeyboardInput.SpecialKeys.RETURN);
            }
        }

        static public void addTest(Window window, string name)
        {
            window.Get<Button>("btnShowTests").Click();

            ListView categories = window.Get<ListView>("catsDataGrid");
            categories.Rows[0].Click();

            ListView tests = window.Get<ListView>("testsDataGrid");
            int testsCount = tests.Rows.Count;

            Button addButton = window.Get<Button>("addStuffButton");

            addButton.Click();
            PopUpMenu menu = window.Popup;
            menu.Item("New test").Click();

            window.Get<TextBox>("testName").Enter(name);
            window.Get<TextBox>("units").Enter("cm");

            Tab tab = window.Get<Tab>("GenderTab");
            tab.SelectTabPage(0);

            Button ratingButton = window.Get<Button>("addRatingsM");
            ratingButton.Click();

            Window modal = window.ModalWindow("Input");
            modal.Get<TextBox>().Enter("0-10");
            modal.Get<Button>("btnDialogOk").Click();

            ListViewCell cell;

            tab = window.Get<Tab>("GenderTab");
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
            women.SelectTabPage(0);
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
        }

        static public void addClientGroupIfNoneExist(Window window, string name)
        {
            window.Get<Button>("btnShowClients").Click();
            ListView groups = window.Get<ListView>("groupDataGrid");
            Button addButton = window.Get<Button>("addStuff");
            PopUpMenu menu;
            if (groups.Rows.Count <= 0)
            {
                addButton.Click();

                menu = window.Popup;
                menu.Item("New group").Click();


                groups.SelectedRows[0].Enter(name);
                window.Keyboard.PressSpecialKey(KeyboardInput.SpecialKeys.RETURN);
            }
        }

        static public void addClient(Window window, string name)
        {
            window.Get<Button>("btnShowClients").Click();

            ListView groups = window.Get<ListView>("groupDataGrid");
            Button addButton = window.Get<Button>("addStuff");
            PopUpMenu menu;

            groups.Rows[0].Click();

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
        }
    }
}
