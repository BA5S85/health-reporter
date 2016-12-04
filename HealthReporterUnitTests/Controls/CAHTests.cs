using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HealthReporter;
using HealthReporter.Utilities;
using HealthReporter.Models;
using HealthReporter.Controls;

namespace HealthReporterUnitTests.Controls
{
    [TestClass]
    public class CAHTests
    {
        [TestInitialize]
        public void InitializeDatabase()
        {
            DatabaseUtility.resetDb();
            DatabaseUtility.checkDb();
        }

        [TestMethod]
        public void FindAge()
        {
            TestCategory category = new TestCategory();
            category.name = "TestCategory";
            category.id = System.Guid.NewGuid().ToByteArray();

            TestCategoryRepository catRepo = new TestCategoryRepository();
            catRepo.Insert(category);

            Test test = new Test();
            test.id = System.Guid.NewGuid().ToByteArray();
            test.categoryId = category.id;
            test.name = "Test";
            test.weight = 1;

            TestRepository testRepo = new TestRepository();
            testRepo.Insert(test);

            RatingRepository ratingRepo = new RatingRepository();

            Rating rating = new Rating();
            rating.testId = test.id;
            rating.age = 0;
            rating.normF = 10;
            rating.normM = 15;

            ratingRepo.Insert(rating);

            rating = new Rating();
            rating.testId = test.id;
            rating.age = 20;
            rating.normF = 20;
            rating.normM = 30;

            ratingRepo.Insert(rating);

            rating = new Rating();
            rating.testId = test.id;
            rating.age = 40;
            rating.normF = 20;
            rating.normM = 30;

            ratingRepo.Insert(rating);

            int age = CAH.findAge(25, test.id);

            Assert.AreEqual(age, 20, 0.001, "findAge returne wrong age");
        }
    }
}