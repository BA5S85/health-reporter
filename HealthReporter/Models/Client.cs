using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Insight.Database;
using HealthReporter.Utilities;
using System.Data.SQLite;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace HealthReporter.Models
{
    class ClientRepository
    {
        public void Insert(Client client)
        {
            var connection = DatabaseUtility.getConnection();
            client.id = System.Guid.NewGuid().ToByteArray();
            
            var res = connection.InsertSql("INSERT INTO clients (id, firstName, lastName, groupId, email, gender, birthDate) values(@id, @firstName, @lastName, @groupId, @email, @gender, @birthDate)", client);
        }

        public IList<Client> FindAll()
        {
            return DatabaseUtility.getConnection().QuerySql<Client>("SELECT * FROM clients");
        }

        public void Delete(Client client)
        {
            var connection = DatabaseUtility.getConnection();
            const string sqlString = "PRAGMA foreign_keys = ON;";
            SQLiteCommand command = new SQLiteCommand(sqlString, connection);
            command.ExecuteNonQuery();
            var res = connection.InsertSql("DELETE from clients where id=@id", client);

        }

        internal IList<Client> SelectClient(byte[] id)
        {
            return DatabaseUtility.getConnection().QuerySql<Client>("SELECT * FROM clients WHERE id='" + id + "'");
        }



        public void Update(Client client)
        {
            var connection = DatabaseUtility.getConnection();

            var res = connection.InsertSql("UPDATE clients set firstName='" + client.firstName + "', lastName='" + client.lastName + "', email='" + client.email + "', gender = '" + client.gender + "', birthDate= '" + client.birthDate + "', updated = CURRENT_TIMESTAMP WHERE id=@id", client);

        }
        public IList<Client> GetClientsByGroupName(Group group)
        {
            return DatabaseUtility.getConnection().QuerySql<Client>("SELECT * FROM clients WHERE groupId = @id", group);
        }

        internal IList<Client> FindSearchResult(string searchBy, Group group)
        {
            return DatabaseUtility.getConnection().QuerySql<Client>("SELECT * FROM clients inner join groups on clients.groupId=groups.id WHERE (clients.firstname LIKE '%" + searchBy + "%' OR clients.lastName LIKE'%" + searchBy + "%') AND groups.name = '" + group.name+ "' AND groups.id=@id" , group);
        }
        internal IList<Client> FindSearchResultAllClients(string searchBy)
        {
            return DatabaseUtility.getConnection().QuerySql<Client>("SELECT * FROM clients inner join groups on clients.groupId=groups.id WHERE (clients.firstname LIKE '%" + searchBy + "%' OR clients.lastName LIKE '%" + searchBy + "%' OR groups.name LIKE '%" + searchBy + "%') ");
        }
    }

    public class Client : INotifyPropertyChanged, IDataErrorInfo
    {

        public byte[] id { get; set; }
        public string updated { get; set; }
        public string uploaded { get; set; }
        public byte[] groupId { get; set; }
        private string _firstName;
        private string _lastName;
        private string _email;
        private string _birthDate;
        private string _gender;
        private string _age;



        public string firstName
        {
            get
            {
                return _firstName;
            }
            set
            {
                _firstName = value;
                OnPropertyChanged("firstName");
            }
        }

        public string lastName
        {
            get
            {
                return _lastName;
            }
            set
            {
                _lastName = value;
                OnPropertyChanged("lastName");
            }
        }
        public string email
        {
            get
            {
                return _email;
            }
            set
            {
                _email = value;
                OnPropertyChanged("email");
            }
        }
        public string gender
        {
            get
            {
                return _gender;
            }
            set
            {
                _gender = value;
                OnPropertyChanged("gender");
            }
        }
        public string birthDate
        {
            get
            {
                return _birthDate;
            }
            set
            {
                _birthDate = value;
                OnPropertyChanged("birthDate");
            }
        }

        public string age
        {
            get
            {

                var today = DateTime.Today;
                DateTime birthday = DateTime.Parse(this.birthDate);
                var age = today.Year - birthday.Year;
                if (birthday > today.AddYears(-age)) age--;
                this._age = age.ToString();
                return _age;


            }

        }





        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region IDataErrorInfo Members
        string IDataErrorInfo.Error
        {
            get
            {
                return null;
            }
        }



        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                return GetValidationError(propertyName);
            }
        }


        #endregion

        #region Validation

        static readonly string[] ValidatedProperties =
       {
            "firstName", "lastName","email", "gender", "birthDate"
        };

        public bool IsValid
        {
            get
            {
                foreach (string property in ValidatedProperties)
                {
                    if (GetValidationError(property) != null)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        string GetValidationError(string propertyName)
        {
            string error = null;

            switch (propertyName)
            {
                case "firstName":
                    error = ValidateClientFirstName();
                    break;
                case "lastName":
                    error = ValidateClientLastName();
                    break;
                case "email":
                    error = ValidateClientEmail();
                    break;
                case "gender":
                    error = ValidateClientGender();
                    break;
                case "birthDate":
                    error = ValidateBirthDate();
                    break;
            }
            return error;
        }

        private string ValidateClientFirstName()
        {
            if (String.IsNullOrWhiteSpace(firstName))
            {
                return "Client first name can not be empty.";
            }
            else
            {
                return null;
            }
        }
        private string ValidateClientLastName()
        {
            if (String.IsNullOrWhiteSpace(lastName))
            {
                return "Client last name can not be empty.";
            }
            else
            {
                return null;
            }
        }
        private string ValidateClientEmail()
        {

            if (!String.IsNullOrWhiteSpace(email) && !Regex.IsMatch(email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"))
            {
                return "Email format is wrong";
            }
            else
            {
                return null;
            }
        }
        private string ValidateClientGender()
        {
            if (String.IsNullOrWhiteSpace(gender))
            {
                return "Client gender can not be empty.";
            }
            else
            {
                return null;
            }
        }
        private string ValidateBirthDate()
        {
            if (String.IsNullOrWhiteSpace(birthDate))
            {
                return "Date of birth can not be empty.";
            }
            else
            {
                return null;
            }
        }
        #endregion

    }


}
