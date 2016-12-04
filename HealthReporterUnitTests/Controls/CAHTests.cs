using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HealthReporter;
using HealthReporter.Utilities;
using HealthReporter.Models;
using HealthReporter.Controls;
using System.Collections.Generic;

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
        public void findScoreMeaning()
        {
            decimal score = 15;
            RatingMeaning targetMeaning = new RatingMeaning() { rating = 1, name = "test 2", normF = 5, normM = 10 };
            IList<RatingMeaning> list = new List<RatingMeaning>();
            list.Add(new RatingMeaning() { rating = 0, name = "test 0", normF = 0, normM = 0 });
            list.Add(targetMeaning);
            list.Add(new RatingMeaning() { rating = 2, name = "test 1", normF = 15, normM = 20 });
            list.Add(new RatingMeaning() { rating = 3, name = "test 2", normF = 25, normM = 30 });

            RatingMeaning result = CAH.findScoreMeaning(score, list, true);

            Assert.AreEqual(result, targetMeaning, "findScoreMeaning returned wrong meaning");
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