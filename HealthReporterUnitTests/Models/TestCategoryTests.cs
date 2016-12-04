using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HealthReporter.Models;

namespace HealthReporterUnitTests.Models
{
    [TestClass]
    public class TestCategoryTests
    {
        [TestMethod]
        public void IsValid_WithValidTestCategory()
        {
            TestCategory category = new TestCategory();
            category.id = System.Guid.NewGuid().ToByteArray();
            category.name = "TestCategory";

            Assert.IsTrue(category.IsValid, "Valid category fails validation");
        }

        [TestMethod]
        public void IsValid_WithInValidTestCategory()
        {
            TestCategory category = new TestCategory();
            category.id = System.Guid.NewGuid().ToByteArray();

            Assert.IsFalse(category.IsValid, "Invalid category passes validation");
        }
    }
}
