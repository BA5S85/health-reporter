using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HealthReporter.Models;

namespace HealthReporterUnitTests.Models
{
    [TestClass]
    public class AppraiserTests
    {
        [TestMethod]
        public void IsValid_WithValidAppraiser()
        {
            Appraiser appraiser = new Appraiser();
            appraiser.id = System.Guid.NewGuid().ToByteArray();
            appraiser.name = "Mati";

            Assert.IsTrue(appraiser.IsValid, "Valid appraisal fails validation");
        }

        [TestMethod]
        public void IsValid_WithInValidAppraisal()
        {
            Appraiser appraiser = new Appraiser();
            appraiser.id = System.Guid.NewGuid().ToByteArray();

            Assert.IsFalse(appraiser.IsValid, "Invalid appraiser passes validation");
        }
    }
}
