using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HealthReporter.Models;

namespace HealthReporterUnitTests.Models
{
    /// <summary>
    /// Summary description for AppraisalTests
    /// </summary>
    [TestClass]
    public class AppraisalTests
    {
        [TestMethod]
        public void IsValid_WithValidAppraisal()
        {
            Appraisal appraisal = new Appraisal();
            appraisal.id = System.Guid.NewGuid().ToByteArray();
            appraisal.appraiserId = System.Guid.NewGuid().ToByteArray();
            appraisal.clientId = System.Guid.NewGuid().ToByteArray();
            appraisal.date = String.Format("{0:yyyy-MM-dd}", DateTime.Now);

            Assert.IsTrue(appraisal.IsValid, "Valid appraisal fails validation");
        }

        [TestMethod]
        public void IsValid_WithInValidAppraisal()
        {
            Appraisal appraisal = new Appraisal();
            appraisal.id = System.Guid.NewGuid().ToByteArray();
            appraisal.appraiserId = System.Guid.NewGuid().ToByteArray();
            appraisal.clientId = System.Guid.NewGuid().ToByteArray();

            Assert.IsFalse(appraisal.IsValid, "Invalid appraisal passes validation");
        }
    }
}
