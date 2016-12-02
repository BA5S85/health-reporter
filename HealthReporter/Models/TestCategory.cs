using System.Collections.Generic;
using Insight.Database;
using HealthReporter.Utilities;
using System;
using System.ComponentModel;

namespace HealthReporter.Models
{
    class TestCategoryRepository
    {
        public void Insert(TestCategory testCategory)
        {
            var connection = DatabaseUtility.getConnection();
            testCategory.id = System.Guid.NewGuid().ToByteArray();
            var res = connection.InsertSql("INSERT INTO test_categories (id, parentId, name, position, uploaded) values(@id, @parentId, @name, @position, @uploaded)", testCategory);
        }

        public IList<TestCategory> FindAll()
        {
            return DatabaseUtility.getConnection().QuerySql<TestCategory>("SELECT * FROM test_categories");
        }

        public IList<TestCategory> GetCategoryByTest(Test test)
        {
            return DatabaseUtility.getConnection().QuerySql<TestCategory>("SELECT * FROM test_categories WHERE id=@categoryId", test);
        }

        public IList<TestCategory> FindRootCategories()
        {
            return DatabaseUtility.getConnection().QuerySql<TestCategory>("SELECT * FROM test_categories WHERE parentId IS NULL");
        }

        public IList<TestCategory> GetCategoryByParent(TestCategory cat)
        {
            return DatabaseUtility.getConnection().QuerySql<TestCategory>("SELECT * FROM test_categories WHERE parentId = @id", cat);
        }
        public IList<TestCategory> GetParent(TestCategory cat)
        {
            return DatabaseUtility.getConnection().QuerySql<TestCategory>("SELECT * FROM test_categories WHERE id = @ParentId", cat);
        }

        public void Delete(TestCategory cat)
        {
            var connection = DatabaseUtility.getConnection().InsertSql("DELETE FROM test_categories WHERE id = @id", cat);
        }

        public void Update(TestCategory cat)
        {
            var connection = DatabaseUtility.getConnection();
            var res = connection.InsertSql("UPDATE test_categories set name='" + cat.name + "', updated = CURRENT_TIMESTAMP WHERE id=@id", cat);
        }
    }

    public class TestCategory : IHasPrimaryKey, INotifyPropertyChanged, IDataErrorInfo
    {
        public byte[] GetPrimaryKey()
        {
            return this.id;
        }

        public string IdAsString()
        {
            string hex = BitConverter.ToString(this.id);
            return hex.Replace("-", "");
        }

        public byte[] id { get; set; }
        public byte[] parentId { get; set; }
        public string name { get; set; }
        public int position { get; set; }
        public string updated { get; set; }
        public string uploaded { get; set; }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged("name");
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

        static readonly string[] ValidatedProperties = { "name" };

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
            return ValidateCategoryName();
        }

        private string ValidateCategoryName()
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                return "A category name cannot be empty";
            }
            else
            {
                return null;
            }
        }
        #endregion
    }


}
