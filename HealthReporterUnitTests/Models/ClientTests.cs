using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HealthReporter.Models;

namespace HealthReporterUnitTests.Models
{
    [TestClass]
    public class ClientTests
    {
        [TestMethod]
        public void IsValid_WithValidClient()
        {
            Client client = new Client();
            client.id = System.Guid.NewGuid().ToByteArray();
            client.firstName = "Peeter";
            client.lastName = "Meeter";
            client.gender = "1";
            client.birthDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now);
            client.email = "peeter@mail.com";

            Assert.IsTrue(client.IsValid, "Valid appraisal fails validation");
        }

        [TestMethod]
        public void IsValid_WithInValidEmail()
        {
            Client client = new Client();
            client.id = System.Guid.NewGuid().ToByteArray();
            client.firstName = "Peeter";
            client.lastName = "Meeter";
            client.gender = "1";
            client.birthDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now);
            client.email = "peeter";

            Assert.IsFalse(client.IsValid, "Client with invalid email passes validation");
        }

        [TestMethod]
        public void IsValid_WithInValidBirthDate()
        {
            Client client = new Client();
            client.id = System.Guid.NewGuid().ToByteArray();
            client.firstName = "Peeter";
            client.lastName = "Meeter";
            client.gender = "1";
            client.email = "peeter@mail.com";

            Assert.IsFalse(client.IsValid, "Client without date passes validation");
        }

        [TestMethod]
        public void IsValid_WithInvalidFirstName()
        {
            Client client = new Client();
            client.id = System.Guid.NewGuid().ToByteArray();
            client.lastName = "Meeter";
            client.gender = "1";
            client.birthDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now);
            client.email = "peeter@mail.com";

            Assert.IsFalse(client.IsValid, "Client without first name passes validation");
        }

        [TestMethod]
        public void IsValid_WithInvalidlastName()
        {
            Client client = new Client();
            client.id = System.Guid.NewGuid().ToByteArray();
            client.firstName = "Peeter";
            client.gender = "1";
            client.birthDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now);
            client.email = "peeter@mail.com";

            Assert.IsFalse(client.IsValid, "Client without last name passes validation");
        }

        [TestMethod]
        public void IsValid_WithInvalidGender()
        {
            Client client = new Client();
            client.id = System.Guid.NewGuid().ToByteArray();
            client.firstName = "Peeter";
            client.lastName = "Meeter";
            client.birthDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now);
            client.email = "peeter@mail.com";

            Assert.IsFalse(client.IsValid, "Client without gender passes validation");
        }
    }
}