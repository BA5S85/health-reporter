﻿using HealthReporter.Utilities;
using Insight.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthReporter.Models
{

    public class AppraisalsRepository
    {
        public void Insert(Appraisal appraisal, Appraiser appraiser, List<Appraisal_tests> tests)
        {
            var connection = DatabaseUtility.getConnection();

            var res = connection.InsertSql("INSERT INTO appraisals (id, appraiserId, clientId, date ) values(@id, @appraiserId, @clientId, @date)", appraisal);
            var res2 = connection.InsertSql("INSERT INTO appraisers (id, name) values(@id, @name)", appraiser);

            
            foreach (Appraisal_tests test in tests)
            {
                var res3 = connection.InsertSql("INSERT INTO appraisal_tests (appraisalId, testId, score, note) values(@appraisalId, @testId, @score, @note)", test);
            }


        }

        public IList<HistoryTableItem> FindAll(Client client)
        {
           return DatabaseUtility.getConnection().QuerySql<HistoryTableItem>("SELECT appraisals.date, tests.name as TestName, tests.categoryId as tCategory, tests.units as Units,  appraisal_tests.score as Score, appraisers.name as AppraisersName,appraisals.id as applId, tests.id as tId  FROM appraisers inner JOIN appraisals ON appraisals.appraiserId = appraisers.id inner JOIN appraisal_tests ON appraisal_tests.appraisalId = appraisals.id inner JOIN tests ON tests.id = appraisal_tests.testId WHERE appraisals.clientId=@id ", client);

        }

        public IList<Test> FindAppraisalTests(Client client)
        {
            return DatabaseUtility.getConnection().QuerySql<Test>("SELECT * FROM appraisals inner JOIN appraisal_tests ON appraisal_tests.appraisalId = appraisals.id inner JOIN tests ON tests.id = appraisal_tests.testId WHERE appraisals.clientId=@id ", client);

        }

    }

    public class Appraisal : INotifyPropertyChanged, IDataErrorInfo
    {
        public byte[] id { get; set; }
        public byte[] appraiserId { get; set; }
        public byte[] clientId { get; set; }
       
        public string updated { get; set; }
        private string _date;

        public string date
        {
            get
            {
                return _date;
            }
            set
            {
                _date = value;
                OnPropertyChanged("date");
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
            "date"
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
                case "date":
                    error = ValidateName();
                    break;

            }
            return error;
        }

        private string ValidateName()
        {
            if (String.IsNullOrWhiteSpace(date))
            {
                return "Appraisal date can not be empty.";
            }
            else
            {
                return null;
            }
        }

        #endregion


    }

    public class HistoryTableItem
    {
        

        public string TestName { get; set; }
        public string Units { get; set; }
        public string date { get; set; }
        public decimal Score { get; set; }
        public string AppraisersName { get; set; }
        public byte[] applId { get; set; }
        public byte[] tId { get; set; }
        public byte[] tCategory { get; set; }
    }


    //Classes for holding all HistoryDatagrid Items
    public class FullHistoryDatagrid
    {
        public string TestName { get; set; }
        public string units { get; set; }
        public byte[] tId { get; set; }
        public List<Date_Score_Appraiser> list { get; set; }


    }

    public class Date_Score_Appraiser
    {
        public string date { get; set; }
        private decimal _score;
        public string appraiser { get; set; }
        public byte[] applId { get; set; }
        public byte[] tId { get; set; }

        public decimal score
        {
            get
            {
                return _score;
            }
            set
            {
                _score = value;
            }
        }
        public override string ToString()
        {
            if (score.ToString() == "0")
            {
                return "";
            }
            else
            {
                return score.ToString();
            }
        }
    }





}
